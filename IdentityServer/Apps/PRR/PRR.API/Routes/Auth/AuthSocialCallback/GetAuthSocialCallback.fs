namespace PRR.API.Routes.Auth.SocialCallback

open FSharpx
open PRR.System.Models
open Common.Domain.Models
open Microsoft.AspNetCore.Http
open Giraffe
open PRR.API.Routes
open Common.Domain.Giraffe
open Common.Utils
open PRR.Domain.Auth.Social.SocialCallback
open PRR.Sys.Models.Social
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.API.Routes.Auth

module GetAuthSocialCallback =

    let private getEnv (ctx: HttpContext): Env =
        let config = getConfig ctx
        let logger = getLogger ctx

        let onSuccessEnv: LogIn.OnSuccess.Env =
            { KeyValueStorage = getKeyValueStorage ctx
              Logger = logger }

        let getSocialLoginEnv: GetSocialLoginItem.Env =
            { KeyValueStorage = getKeyValueStorage ctx
              Logger = logger }

        { DataContext = getDataContext ctx
          PasswordSalter = getPasswordSalter ctx
          CodeGenerator = getHash ctx
          Logger = getLogger ctx
          CodeExpiresIn = config.Auth.Jwt.CodeExpiresIn
          SSOExpiresIn = config.Auth.SSOCookieExpiresIn
          GetSocialLoginItem = getSocialLoginItem getSocialLoginEnv
          OnSuccess = LogIn.OnSuccess.onSuccess onSuccessEnv
          HttpRequestFun = getHttpRequestFun ctx
          SocialCallbackUrl = config.Auth.Social.CallbackUrl
          PerimeterSocialClientSecretKeys =
              { Github = config.Auth.PerimeterSocialProviders.Github.SecretKey
                Google = config.Auth.PerimeterSocialProviders.Google.SecretKey
                Twitter = config.Auth.PerimeterSocialProviders.Twitter.SecretKey } }


    let private handle next ctx =
        let env = getEnv ctx
        task {
            let data = bindQueryString ctx
            let sso = bindCookie "sso" ctx
            let! result = socialCallback env data sso
            // TODO : Error handler
            return! redirectTo false result.RedirectUrl next ctx
        }

    let createRoute () = GET >=> handle
