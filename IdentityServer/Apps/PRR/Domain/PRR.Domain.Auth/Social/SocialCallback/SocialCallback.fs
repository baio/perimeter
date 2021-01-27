namespace PRR.Domain.Auth.Social.SocialCallback

open Common.Domain.Models
open System.Threading.Tasks
open Common.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open Common.Domain.Utils
open PRR.Data.Entities
open PRR.Domain.Auth.Common.KeyValueModels
open PRR.Domain.Auth.LogIn.Authorize
open System.Linq
open Microsoft.Extensions.Logging

[<AutoOpen>]
module Social =

    let private getCommonSocialConnectionSecret (dataContext: DbDataContext) clientId socialType =
        let socialTypeName = socialType2Name socialType
        query {
            for app in dataContext.Applications do
                join sc in dataContext.SocialConnections on (app.DomainId = sc.DomainId)
                where
                    (app.ClientId = clientId
                     && sc.SocialName = socialTypeName)
                select sc.ClientSecret
        }
        |> toSingleAsync

    let private getPerimeterSocialConnectionSecret (keys: PerimeterSocialClientSecretKeys) =
        function
        | Github -> keys.Github
        | Twitter -> keys.Twitter
        | Google -> keys.Google

    let private getSocialConnectionSecret keys (dataContext: DbDataContext) clientId socialType =
        // Since there is no app to manage perimeter admin data itself,
        // setup social providers for perimeter runtime through the environment configuration
        if clientId = DEFAULT_CLIENT_ID then
            getPerimeterSocialConnectionSecret keys socialType
            |> Task.FromResult
        else
            getCommonSocialConnectionSecret dataContext clientId socialType


    let private identityToUser (ident: SocialIdentity) =
        let (firstName, lastName) = splitName ident.Name
        User(FirstName = firstName, LastName = lastName, Email = ident.Email)

    let private getExistentUserWithSocials (dataContext: DbDataContext) email =
        query {
            for user in dataContext.Users do
                where (user.Email = email)
                select (user.Id, user.SocialIdentities.Select(fun x -> x.SocialName))
        }
        |> toSingleOptionAsync

    let private createUserAndSocialIdentity (dataContext: DbDataContext) (ident: SocialIdentity) =
        task {
            match! getExistentUserWithSocials dataContext ident.Email with
            | Some (userId, socialTypes) ->
                match socialTypes.Contains ident.SocialName with
                // user and social type already exists
                | true -> return userId
                | false ->
                    // user exist but social type is new
                    // add social type
                    ident.UserId <- userId
                    ident |> add dataContext
                    do! dataContext |> saveChangesAsync
                    return userId
            | None ->
                // user and social type is not exists, new user
                // create user and social type
                let user = identityToUser ident
                ident.User <- user
                user |> add dataContext
                ident |> add dataContext
                do! dataContext |> saveChangesAsync
                return user.Id
        }

    let private getSuccessRedirectUrl (loginResult: PRR.Domain.Auth.LogIn.Models.Result) =
        sprintf "%s?code=%s&state=%s" loginResult.RedirectUri loginResult.Code loginResult.State

    let private getSocialLoginItem env state =
        task {
            match! env.KeyValueStorage.GetValue<SocialLoginKV> state None with
            | Ok item ->
                env.Logger.LogInformation("SocialLoginItem ${@item} found", item)
                return item
            | Error err ->
                env.Logger.LogWarning("SocialLoginItem is not found for ${state} with ${@error}", state, err)
                return raise (unAuthorized "state is not found")
        }


    let socialCallback (env: Env) (data: Data) (ssoToken: string option) =
        let logger = env.Logger
        task {
            logger.LogInformation("SocialCallback starts ${@data}", data)
            // Get stored before social login item
            let! item = getSocialLoginItem env data.State

            // get social connection secret
            let! secret =
                getSocialConnectionSecret
                    env.PerimeterSocialClientSecretKeys
                    env.DataContext
                    item.DomainClientId
                    item.Type

            let! ident =
                getSocialIdentity
                    env.SocialCallbackUrl
                    item.Type
                    env.HttpRequestFun
                    item.SocialClientId
                    secret
                    data.Code

            // create user and social identity (if still not created)
            let! userId = createUserAndSocialIdentity env.DataContext ident

            logger.LogInformation("User with ${userId} and social identity created", userId)

            // login user as usual with data from social provider
            let loginData: LoginData =
                { UserId = userId
                  ClientId = item.DomainClientId
                  ResponseType = item.ResponseType
                  State = item.State
                  RedirectUri = item.RedirectUri
                  Scope = item.Scope
                  Email = ident.Email
                  CodeChallenge = item.CodeChallenge
                  CodeChallengeMethod = item.CodeChallengeMethod }

            logger.LogInformation("${@loginData} created", { loginData with CodeChallenge = "***" })

            let env': PRR.Domain.Auth.LogIn.Models.Env =
                { DataContext = env.DataContext
                  CodeGenerator = env.CodeGenerator
                  PasswordSalter = env.PasswordSalter
                  CodeExpiresIn = env.CodeExpiresIn
                  SSOExpiresIn = env.SSOExpiresIn
                  Logger = env.Logger
                  KeyValueStorage = env.KeyValueStorage }

            let social: Social =
                { Id = ident.SocialId
                  Type = item.Type }

            let! res = logInUser env' ssoToken loginData (Some social)

            logger.LogInformation("loginUser success ${@res}", res)

            // get redirect url
            let redirectUrl = getSuccessRedirectUrl res

            logger.LogInformation("${@redirectUrl} is ready", redirectUrl)

            (*
            let loginItem = { loginItem with Social = Some social }

            let ssoItem =
                ssoItem
                |> Option.map (fun ssoItem -> { ssoItem with Social = Some social })

            let successData = loginItem, ssoItem

            logger.LogInformation("${@successData} is ready", successData)

            let env': PRR.Domain.Auth.LogIn.OnSuccess.Env =
                { KeyValueStorage = env.KeyValueStorage
                  Logger = env.Logger }

            do! PRR.Domain.Auth.LogIn.OnSuccess.onSuccess env' (successData)
            *)

            let result: Result =
                { RedirectUrl = redirectUrl
                  SocialLoginToken = item.Token }

            logger.LogInformation("${@result} is ready", { result with SocialLoginToken = "***" })

            return result
        }
