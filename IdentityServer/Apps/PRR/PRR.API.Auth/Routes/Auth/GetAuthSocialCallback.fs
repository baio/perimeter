namespace PRR.API.Auth.Routes

open PRR.Domain.Models
open Microsoft.AspNetCore.Http
open Giraffe
open DataAvail.Giraffe.Common
open PRR.Domain.Auth.Social.SocialCallback
open FSharp.Control.Tasks.V2.ContextInsensitive

module GetAuthSocialCallback =

    let private getEnv (ctx: HttpContext): Env =
        let config = getConfig ctx

        { DataContext = getDataContext ctx
          PasswordSalter = getPasswordSalter ctx
          CodeGenerator = getHash ctx
          Logger = getLogger ctx
          CodeExpiresIn = config.Auth.Jwt.CodeExpiresIn
          SSOExpiresIn = config.Auth.SSOCookieExpiresIn
          KeyValueStorage = getKeyValueStorage ctx
          HttpRequestFun = getHttpRequestFun ctx
          SocialCallbackUrl = config.Auth.Social.CallbackUrl
          PerimeterSocialClientSecretKeys =
              { Github = config.Auth.PerimeterSocialProviders.Github.SecretKey
                Google = config.Auth.PerimeterSocialProviders.Google.SecretKey
                Twitter = config.Auth.PerimeterSocialProviders.Twitter.SecretKey } }


    let handler next ctx =
        let env = getEnv ctx
        task {
            let data = bindQueryString ctx
            let sso = bindCookie "sso" ctx
            let! result = socialCallback env data sso
            // TODO : Error handler
            return! redirectTo false result.RedirectUrl next ctx
        }
