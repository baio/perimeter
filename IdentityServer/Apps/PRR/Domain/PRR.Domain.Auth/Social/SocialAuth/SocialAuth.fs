﻿namespace PRR.Domain.Auth.Social

open System.Threading.Tasks
open Common.Domain.Models
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open Common.Domain.Utils
open PRR.Data.Entities
open PRR.Sys.Models.Social
open PRR.Domain.Auth.Constants

[<AutoOpen>]
module Social =

    type private SocialInfo =
        { ClientId: string
          Attributes: string seq
          Permissions: string seq }

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



    let private getRedirectUrl token callbackUri (info: SocialInfo) =
        function
        | Github ->
            // https://docs.github.com/en/free-pro-team@latest/developers/apps/authorizing-oauth-apps
            sprintf
                "https://github.com/login/oauth/authorize?client_id=%s&redirect_uri=%s&state=%s"
                info.ClientId
                callbackUri
                token

    let socialAuth (env: Env, data: Data) =
        task {

            // Social name to social type
            let socialType = socialName2Type data.Social_Name

            // Collect info for social redirect url such as social client id, attributes and scopes
            let! socialInfo =
                getSocialConnectionInfo env.PerimeterSocialClientIds env.DataContext data.Client_Id socialType

            // Generate token it will be used as state for redirect url
            let token = env.HashProvider()

            let redirectUrl =
                getRedirectUrl token env.SocialCallbackUrl socialInfo socialType

            // Store login data they will be used when callback hit back
            let cmd =
                { Token = token
                  ExpiresIn = env.SocialCallbackExpiresIn
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
                |> SocialLoginAddCommand

            return (redirectUrl, cmd)
        }
