namespace PRR.API.Auth.Routes

open Microsoft.AspNetCore.Http
open Giraffe
open DataAvail.Giraffe.Common
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Domain.Auth.LogIn.Social.SocialAuth
open PRR.API.Auth.Routes.Helpers
open Microsoft.Extensions.Logging

module PostAuthSocial =

    let getEnv (ctx: HttpContext): Env =
        let config = getConfig ctx

        { DataContext = getDataContext ctx
          HashProvider = getHash ctx
          SocialCallbackUrl = config.Auth.Social.CallbackUrl
          SocialCallbackExpiresIn = config.Auth.Social.CallbackExpiresIn
          Logger = getLogger ctx
          KeyValueStorage = getKeyValueStorage ctx
          HttpRequestFun = getHttpRequestFun ctx
          PerimeterSocialClientIds =
              { Github = config.Auth.PerimeterSocialProviders.Github.ClientId
                Google = config.Auth.PerimeterSocialProviders.Google.ClientId
                Twitter =
                    config.Auth.PerimeterSocialProviders.Twitter.ClientId,
                    config.Auth.PerimeterSocialProviders.Twitter.SecretKey } }

    let handler next ctx =
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
