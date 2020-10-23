namespace PRR.Domain.Auth.Social

open Common.Domain.Models
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open Common.Domain.Utils
open PRR.Data.Entities
open PRR.Sys.Models.Social

[<AutoOpen>]
module Social =

    type private SocialInfo =
        { ClientId: string
          Attributes: string seq
          Permissions: string seq }

    let private getSocialConnectionInfo (dataContext: DbDataContext) clientId socialType =
        query {
            for app in dataContext.Applications do
                join sc in dataContext.SocialConnections on (app.DomainId = sc.DomainId)
                where
                    (app.ClientId = clientId
                     && sc.SocialName = socialType)
                select
                    { ClientId = sc.ClientId
                      Attributes = sc.Attributes
                      Permissions = sc.Permissions }
        }
        |> toSingleAsync


    let private getRedirectUrl token callbackUri (info: SocialInfo) =
        function
        | Github ->
            // https://docs.github.com/en/free-pro-team@latest/developers/apps/authorizing-oauth-apps
            sprintf
                "https://github.com/login/oauth/authorize?client_id=%s&redirect_uri=%s&state=%s"
                info.ClientId
                callbackUri
                token

    let private socialName2Type socialName =
        match socialName with
        | "github" -> SocialType.Github
        | _ ->
            raise
                (sprintf "Social [%s] is not found" socialName
                 |> exn)

    let socialAuth (env: Env, data: Data) =
        task {

            // Social name to social type
            let socialType = socialName2Type data.Social_Name

            // Collect info for social redirect url such as social client id, attributes and scopes
            let! socialInfo = getSocialConnectionInfo env.DataContext data.Client_Id data.Social_Name

            // Generate token it will be used as state for redirect url
            let token = env.HashProvider()

            let redirectUrl =
                getRedirectUrl token env.SocialCallbackUrl socialInfo socialType

            // Store login data they will be used when callback hit back
            let cmd =
                { Token = token
                  SocialClientId = socialInfo.ClientId
                  DomainClientId = data.Client_Id
                  Type = socialType
                  ResponseType = data.Response_Type
                  State = data.State
                  RedirectUri = data.Redirect_Uri
                  Scope = data.Scope
                  CodeChallenge = data.Code_Challenge
                  CodeChallengeMethod = data.Code_Challenge_Method }
                |> SocialLoginAddCommand

            return (redirectUrl, cmd)
        }
