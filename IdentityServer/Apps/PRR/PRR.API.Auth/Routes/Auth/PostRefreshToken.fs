namespace PRR.API.Routes.Auth

open PRR.Domain.Models
open PRR.Domain.Auth
open PRR.Domain.Auth.RefreshToken
open Giraffe
open DataAvail.Giraffe.Common
open PRR.API.Routes
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.Extensions.Logging
open DataAvail.Http.Exceptions

module internal PostRefreshToken =

    let getEnv ctx =

        let config = getConfig ctx

        { DataContext = getDataContext ctx
          HashProvider = getHash ctx
          Logger = getLogger ctx
          JwtConfig = config.Auth.Jwt
          KeyValueStorage = getKeyValueStorage ctx
          TokenExpiresIn = config.Auth.ResetPasswordTokenExpiresIn }

    let handler' ctx =
        let env = getEnv ctx

        let logger = env.Logger

        let bearer = bindAuthorizationBearerHeader ctx

        let bearer =
            match bearer with
            | Some bearer -> bearer
            | None ->
                logger.LogWarning("Bearer is not found in Authorization header")
                raise (UnAuthorized(Some "Bearer is not found in Authorization header"))

        task {
            let! data = bindJsonAsync ctx
            let data = { RefreshToken = data.RefreshToken }
            return! refreshToken env bearer data
        }

    let handler: HttpHandler = wrapHandlerOK handler'
