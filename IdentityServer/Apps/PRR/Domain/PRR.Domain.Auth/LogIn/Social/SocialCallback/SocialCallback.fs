namespace PRR.Domain.Auth.LogIn.Social.SocialCallback

// open PRR.Domain.Auth.LogIn.Authorize
open DataAvail.Common
open PRR.Domain.Models
open System.Threading.Tasks
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open PRR.Data.Entities
open PRR.Domain.Auth.Common.KeyValueModels
open PRR.Domain.Auth.LogIn.Authorize.Authorize
open System.Linq
open Microsoft.Extensions.Logging
open DataAvail.EntityFramework.Common
open DataAvail.Http.Exceptions
open DataAvail.Common.Option
open PRR.Domain.Auth.LogIn.Social.SocialCallback.Identities
open PRR.Domain.Auth.LogIn.Authorize.LogInUser

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

    let private getSuccessRedirectUrl (loginResult: PRR.Domain.Auth.LogIn.Authorize.Models.AuthorizeResult) =
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


    let getOAuth2StateAndCode (data: Data) =
        maybe {
            let! state = data |> Seq.tryFind (fun (a, _) -> a = "state")
            let! code = data |> Seq.tryFind (fun (a, _) -> a = "code")
            return (state, code)
        }

    let getOauth1aStateAndCode (data: Data) =
        maybe {
            let! code =
                data
                |> Seq.tryFind (fun (a, _) -> a = "oauth_verifier")

            let! state =
                data
                |> Seq.tryFind (fun (a, _) -> a = "oauth_token")

            return (state, code)
        }

    let getStateAndCode (data: Data) =
        seq {
            getOAuth2StateAndCode data
            getOauth1aStateAndCode data
        }
        |> Seq.choose (id)
        |> Seq.tryHead
        |> Option.map (fun ((_, state), (_, code)) -> state, code)

    let socialCallback (env: Env) (data: Data) (ssoCookie) =

        // TODO : Validate data !

        let logger = env.Logger
        logger.LogInformation("SocialCallback starts ${@data}", data)

        let (state, code) =
            match getStateAndCode data with
            | Some (state, code) ->
                logger.LogDebug("State and code is found ${state} ${code}", state, code)
                state, code
            | None ->
                logger.LogError("State or code is not found")
                raise (unAuthorized "State or code not found")

        task {
            // Get stored before social login item
            let! item = getSocialLoginItem env state

            // get social connection secret
            let! secret =
                getSocialConnectionSecret
                    env.PerimeterSocialClientSecretKeys
                    env.DataContext
                    item.DomainClientId
                    item.Type


            let! ident =
                match item.Type with
                | Github -> getGithubSocialIdentity env.HttpRequestFun item.SocialClientId secret code
                | Google ->
                    getGoogleSocialIdentity env.SocialCallbackUrl env.HttpRequestFun item.SocialClientId secret code
                | Twitter ->
                    // Important state and code reversed
                    getTwitterSocialIdentity env.Logger env.HttpRequestFun item.SocialClientId secret code state

            logger.LogInformation("Identity ${ident} created for flow", ident)

            // create user and social identity (if still not created)
            let! userId = createUserAndSocialIdentity env.DataContext ident

            logger.LogInformation("User with ${userId} and social identity created", userId)

            // login user as usual with data from social provider
            let loginData: LoginData =
                { UserId = userId
                  ClientId = item.DomainClientId
                  ResponseType = item.ResponseType
                  State = item.State
                  Nonce = item.Nonce
                  RedirectUri = item.RedirectUri
                  Scope = item.Scope
                  Email = ident.Email
                  CodeChallenge = item.CodeChallenge
                  CodeChallengeMethod = item.CodeChallengeMethod }

            logger.LogDebug("${@loginData} created")

            let env': PRR.Domain.Auth.LogIn.Authorize.Models.Env =
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

            let! res = logInUser env' ssoCookie loginData (Some social)

            logger.LogInformation("loginUser success ${@res}", res)

            // get redirect url
            let redirectUrl = getSuccessRedirectUrl res

            logger.LogInformation("${@redirectUrl} is ready", redirectUrl)

            let result: Result =
                { RedirectUrl = redirectUrl
                  SocialLoginToken = item.Token }

            logger.LogDebug("${@result} is ready", result)

            return result
        }
