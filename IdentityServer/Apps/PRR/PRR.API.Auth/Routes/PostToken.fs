namespace PRR.API.Auth.Routes

open Giraffe
open DataAvail.Giraffe.Common
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Domain.Auth.LogIn.TokenAuthorizationCode

module PostToken =

    let getEnv ctx =
        let config = getConfig ctx
        { DataContext = getDataContext ctx
          HashProvider = getHash ctx
          Sha256Provider = getSHA256 ctx
          RefreshTokenExpiresIn = config.Auth.RefreshTokenExpiresIn
          JwtConfig = config.Auth.Jwt
          Logger = getLogger ctx
          KeyValueStorage = getKeyValueStorage ctx
          PublishEndpoint = getPublishEndpoint ctx }

    let private handler' ctx =
        task {
            let env = getEnv ctx

            let! data = bindJsonAsync ctx

            let! result = tokenAuthorizationCode env data

            return result
        }

    let handler: HttpHandler = wrapHandlerOK handler'
