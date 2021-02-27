namespace PRR.API.Auth.Routes

open PRR.Domain.Auth.LogIn.RefreshToken
open Giraffe
open DataAvail.Giraffe.Common
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.Extensions.Logging
open DataAvail.Http.Exceptions

module internal PostRefreshTokenHandler =

    let getEnv ctx =

        let config = getConfig ctx

        { DataContext = getDataContext ctx
          HashProvider = getHash ctx
          Logger = getLogger ctx
          JwtConfig = config.Auth.Jwt
          KeyValueStorage = getKeyValueStorage ctx
          TokenExpiresIn = config.Auth.ResetPasswordTokenExpiresIn }

    let handler ctx data =
        let env = getEnv ctx

        let logger = env.Logger

        let bearer = bindAuthorizationBearerHeader ctx

        let bearer =
            match bearer with
            | Some bearer -> bearer
            | None ->
                logger.LogWarning("Bearer is not found in Authorization header")
                raise (UnAuthorized(Some "Bearer is not found in Authorization header"))

        refreshToken env bearer data
