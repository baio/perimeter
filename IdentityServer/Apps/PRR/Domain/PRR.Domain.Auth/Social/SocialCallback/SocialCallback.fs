﻿namespace PRR.Domain.Auth.SocialCallback

open System.Net.Http
open System.Threading.Tasks
open Common.Domain.Models
open Common.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open HttpFs.Client
open Newtonsoft.Json
open PRR.Data.DataContext
open Common.Domain.Utils
open PRR.Data.Entities
open PRR.Domain.Auth.LogIn.Authorize
open PRR.Sys.Models.Social
open Hopac
open PRR.System.Models
open System.Linq

[<AutoOpen>]
module Social =

    type GithubCodeResponse = { access_token: string }

    type GithubUserResponse =
        { id: int
          avatar_url: string
          email: string
          name: string }

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

    let private getPerimeterSocialConnectionSecret keys =
        function
        | Github -> keys.Github


    let private getSocialConnectionSecret keys (dataContext: DbDataContext) clientId socialType =
        // Since there is no app to manage perimeter admin data itself,
        // setup social providers for perimeter runtime through the environment configuration
        match clientId with
        | "__DEFAULT_CLIENT_ID__" ->
            getPerimeterSocialConnectionSecret keys socialType
            |> Task.FromResult
        | _ -> getCommonSocialConnectionSecret dataContext clientId socialType

    let private getGithubCodeResponse (httpRequestFun: HttpRequestFun) clientId secret code =
        task {
            let request: HttpRequest =
                { Uri = "https://github.com/login/oauth/access_token"
                  Method = HttpRequestMethodPOST
                  QueryStringParams =
                      seq {
                          ("client_id", clientId.ToString())
                          ("client_secret", secret)
                          ("code", code)
                      }
                  Headers = seq { ("Accept", "application/json") } }

            let! content = httpRequestFun request

            return JsonConvert.DeserializeObject<GithubCodeResponse> content
        }


    let private getGithubUserResponse (httpRequestFun: HttpRequestFun) token =
        task {

            let request: HttpRequest =
                { Uri = "https://api.github.com/user"
                  Method = HttpRequestMethodGET
                  QueryStringParams = Seq.empty
                  Headers =
                      seq {
                          ("Accept", "application/json")
                          ("Authorization", (sprintf "token %s" token))
                          ("User-Agent", "Perimeter-API")
                      } }

            let! content = httpRequestFun request

            return JsonConvert.DeserializeObject<GithubUserResponse> content
        }

    let private mapSocialUserResponseToIdentity userResponse =
        let socialName = socialType2Name SocialType.Github
        SocialIdentity
            (Name = userResponse.name,
             Email = userResponse.email,
             SocialName = socialName,
             SocialId = userResponse.id.ToString())

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
            match! env.GetSocialLoginItem state with
            | Some item -> return item
            | None -> return raise NotFound
        }

    let socialCallback (env: Env, data: Data, ssoToken: string option) =

        task {

            // Get stored before social login item
            let! item = getSocialLoginItem env data.State

            // get social connection secret
            let! secret =
                getSocialConnectionSecret
                    env.PerimeterSocialClientSecretKeys
                    env.DataContext
                    item.DomainClientId
                    item.Type

            // request social access token by clientId, secret and code from callback
            let! codeResponse = getGithubCodeResponse env.HttpRequestFun item.SocialClientId secret data.Code

            // get github user by received access token
            let! userResponse = getGithubUserResponse env.HttpRequestFun codeResponse.access_token

            // create user and social identity (if still not created)
            let ident =
                mapSocialUserResponseToIdentity userResponse

            let! userId = createUserAndSocialIdentity env.DataContext ident

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

            let env': PRR.Domain.Auth.LogIn.Models.Env =
                { DataContext = env.DataContext
                  CodeGenerator = env.CodeGenerator
                  PasswordSalter = env.PasswordSalter
                  CodeExpiresIn = env.CodeExpiresIn
                  SSOExpiresIn = env.SSOExpiresIn }

            let! (res, evt) = logInUser env' ssoToken loginData

            // get redirect url
            let redirectUrl = getSuccessRedirectUrl res

            // prepare result
            let (loginItem, ssoItem) =
                match evt with
                | UserLogInSuccessEvent (loginItem, ssoItem) -> (loginItem, ssoItem)
                | _ -> raise Unexpected'

            let social: Social =
                { Id = ident.SocialId
                  Type = item.Type }

            let result: Result =
                { RedirectUrl = redirectUrl
                  SocialLoginToken = item.Token
                  LoginItem = { loginItem with Social = Some social }
                  SSOItem =
                      ssoItem
                      |> Option.map (fun ssoItem -> { ssoItem with Social = Some social }) }

            return result
        }
