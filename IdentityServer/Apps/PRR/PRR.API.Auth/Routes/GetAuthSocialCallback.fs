namespace PRR.API.Auth.Routes

open PRR.Domain.Models
open Microsoft.AspNetCore.Http
open Giraffe
open DataAvail.Giraffe.Common
open PRR.Domain.Auth.LogIn.Social.SocialCallback
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.Extensions.Logging

module GetAuthSocialCallback =

    let private getEnv (ctx: HttpContext): Env =
        let config = getConfig ctx
        let getHash = getHash ctx
        let logger = getLogger ctx

        { DataContext = getDataContext ctx
          PasswordSalter = getPasswordSalter ctx
          CodeGenerator = getHash
          Logger = logger
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
            let data = bindQueryStringFields ctx
            let! result = socialCallback env data
            // TODO : Error handler
            return! redirectTo false result.RedirectUrl next ctx
        }
