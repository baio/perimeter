namespace PRR.API.Routes.Auth.AuthorizeToken

open Giraffe
open Common.Domain.Giraffe
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.API.Routes
open PRR.Domain.Auth.LogInToken

module PostAuthorizeToken =

    let getEnv ctx =
        let config = getConfig ctx
        { DataContext = getDataContext ctx
          HashProvider = getHash ctx
          Sha256Provider = getSHA256 ctx
          SSOCookieExpiresIn = config.Auth.SSOCookieExpiresIn
          JwtConfig = config.Auth.Jwt
          Logger = getLogger ctx
          GetCodeItem = getCodeItem ctx
          OnSuccess = onSuccess (getCQRSSystem ctx) }

    let private handler ctx =
        task {
            let env = getEnv ctx

            let! data = bindJsonAsync ctx

            return! logInToken env data
        }

    let createRoute () = POST >=> (wrapHandlerOK handler)
