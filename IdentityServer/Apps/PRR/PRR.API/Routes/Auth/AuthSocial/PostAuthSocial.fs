namespace PRR.API.Routes.Auth.Social

open Microsoft.AspNetCore.Http
open Giraffe
open PRR.API.Routes
open Common.Domain.Giraffe
open Common.Utils
open ReaderTask
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Domain.Auth.Social.SocialAuth
open PRR.Sys.Models.Social
open PRR.API.Routes.Auth.Helpers
open Microsoft.Extensions.Logging

module PostAuthSocial =

    let getEnv (ctx: HttpContext): Env =
        let config = getConfig ctx
        let logger = getLogger ctx
        let kvStorage = getKeyValueStorage ctx

        let onSuccessEnv: OnSuccess.Env =
            { KeyValueStorage = kvStorage
              Logger = logger }

        { DataContext = getDataContext ctx
          HashProvider = getHash ctx
          SocialCallbackUrl = config.Auth.Social.CallbackUrl
          SocialCallbackExpiresIn = config.Auth.Social.CallbackExpiresIn
          OnSuccess = onSuccess onSuccessEnv
          Logger = getLogger ctx
          PerimeterSocialClientIds =
              { Github = config.Auth.PerimeterSocialProviders.Github.ClientId
                Google = config.Auth.PerimeterSocialProviders.Google.ClientId
                Twitter = config.Auth.PerimeterSocialProviders.Twitter.ClientId } }

    let handle next ctx =
        let env = getEnv ctx
        task {
            let! data = bindFormAsync ctx

            try
                let! redirectUrl = socialAuth env data
                return! redirectTo false redirectUrl next ctx
            with ex ->
                let refererUrl = getRefererUrl ctx

                let redirectUrlError =
                    getExnRedirectUrl (refererUrl, data.Redirect_Uri) ex

                env.Logger.LogWarning("Redirect on ${@error} to ${redirectUrlError}", ex, redirectUrlError)

                return! redirectTo false redirectUrlError next ctx
        }

    let createRoute () = POST >=> handle
