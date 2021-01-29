namespace PRR.Domain.Auth.Social.SocialAuth

open System.Threading.Tasks
open PRR.Domain.Models
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open PRR.Domain.Auth.Common
open Microsoft.Extensions.Logging
open DataAvail.KeyValueStorage.Core
open DataAvail.EntityFramework.Common
open DataAvail.Http.Exceptions

[<AutoOpen>]
module SocialAuth =

    let private getCommonSocialConnectionInfo (dataContext: DbDataContext) clientId socialType =

        let socialTypeName = socialType2Name socialType

        query {
            for app in dataContext.Applications do
                join sc in dataContext.SocialConnections on (app.DomainId = sc.DomainId)
                where
                    (app.ClientId = clientId
                     && sc.SocialName = socialTypeName)
                select
                    { ClientId = sc.ClientId
                      Attributes = sc.Attributes
                      Permissions = sc.Permissions }
        }
        |> toSingleAsync

    let private getPerimeterSocialClientId (ids: PerimeterSocialClientIds) =
        function
        | Github -> ids.Github
        | Google -> ids.Google
        | Twitter -> ids.Twitter

    let private getPerimeterSocialConnection ids =
        getPerimeterSocialClientId ids
        >> fun id ->
            ({ ClientId = id
               Attributes = []
               Permissions = [] })

    let private getSocialConnectionInfo (ids: PerimeterSocialClientIds) (dataContext: DbDataContext) clientId socialType =
        // Since there is no app to manage perimeter admin data itself,
        // setup social providers for perimeter runtime through the environment configuration
        match clientId with
        | "__DEFAULT_CLIENT_ID__" ->
            getPerimeterSocialConnection ids socialType
            |> Task.FromResult
        | _ -> getCommonSocialConnectionInfo dataContext clientId socialType


    let socialAuth (env: Env) (data: Data) =
        let logger = env.Logger
        task {
            logger.LogInformation("SocialAuth with ${@data}", { data with Code_Challenge = "***" })
            // Social name to social type
            let socialType = socialName2Type data.Social_Name

            // Collect info for social redirect url such as social client id, attributes and scopes
            let! socialInfo =
                getSocialConnectionInfo env.PerimeterSocialClientIds env.DataContext data.Client_Id socialType

            logger.LogInformation("${@socialInfo} found", socialInfo)

            // Generate token it will be used as state for social redirect url
            let token = env.HashProvider()

            let socialRedirectUrl =
                getSocialRedirectUrl token env.SocialCallbackUrl socialInfo.ClientId socialType

            logger.LogInformation("${socialRedirectUrl} created", socialRedirectUrl)

            // Store login data they will be used when callback hit back
            let data: SocialLoginKV =
                { Token = token
                  SocialClientId = socialInfo.ClientId
                  // The client id here must stay __DEFAULT_CLIENT_ID__ if it was so, since it will be used further in authorization flow
                  DomainClientId = data.Client_Id
                  Type = socialType
                  ResponseType = data.Response_Type
                  State = data.State
                  RedirectUri = data.Redirect_Uri
                  Scope = data.Scope
                  CodeChallenge = data.Code_Challenge
                  CodeChallengeMethod = data.Code_Challenge_Method }

            logger.LogInformation("${successData} ready", data)

            let expiresIn =
                System.DateTime.UtcNow.AddMinutes(float env.SocialCallbackExpiresIn)

            let options =
                { addValueDefaultOptions with
                      ExpiresAt = (Some expiresIn) }

            let! result = env.KeyValueStorage.AddValue token data (Some options)

            match result with
            | Result.Error AddValueError.KeyAlreadyExists ->
                env.Logger.LogError("Token ${token} already exists in Social LogIn storage", token)
                return raise (Unexpected')
            | _ -> ()

            return socialRedirectUrl
        }
