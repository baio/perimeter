namespace PRR.API.Routes.Auth.RefreshToken

open Common.Domain.Models
open PRR.Domain.Auth
open PRR.Domain.Auth.RefreshToken
open Giraffe
open Common.Domain.Giraffe
open PRR.API.Routes
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.Extensions.Logging

module internal PostRefreshToken =

    let getEnv ctx =
        let config = getConfig ctx

        { DataContext = getDataContext ctx
          HashProvider = getHash ctx
          Logger = getLogger ctx
          JwtConfig = config.Auth.Jwt
          OnSuccess = onSuccess (getCQRSSystem ctx)
          GetTokenItem = getTokenItem ctx }

    let handler ctx =
        let env = getEnv ctx

        let logger = env.Logger        

        let bearer = bindAuthorizationBearerHeader ctx

        let bearer =
            match bearer with
            | Some bearer -> bearer
            | None -> 
                logger.LogWarning("Bearer is not found in Authorization header")
                raise (UnAuthorized (Some "Bearer is not found in Authorization header"))

        task {
            let! data = bindJsonAsync ctx
            let data = { RefreshToken = data.RefreshToken }
            return! refreshToken env bearer data
        }

    let createRoute () =
        POST
        >=> choose [ route "/refresh-token" >=> wrapHandlerOK handler ]
