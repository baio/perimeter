namespace PRR.Domain.Auth.LogOut

open PRR.Data.DataContext
open PRR.Data.Entities
open PRR.Domain.Auth
open PRR.Domain.Auth.LogIn.TokenAuthorizationCode
open PRR.Domain.Models
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.IdentityModel.Tokens
open PRR.Domain.Auth.Common.KeyValueModels
open PRR.Domain.Auth.RefreshToken
open System.Security.Claims
open System.Text
open System.Threading.Tasks
open DataAvail.Http.Exceptions
open DataAvail.Common.Option
open DataAvail.EntityFramework.Common
open PRR.Domain.Auth.Common
open PRR.Domain.Auth.Common.Constants
open Microsoft.Extensions.Logging

[<AutoOpen>]
module LogOut =

    type SigningParams =
        | RS256 of string
        | HS256 of string

    let private getSigningParams (dataContext: DbDataContext) jwtConfig clientId =
        task {
            let! result =
                query {
                    for app in dataContext.Applications do
                        where (app.ClientId = clientId)

                        select
                            (app.Domain.SigningAlgorithm,
                             app.Domain.HS256SigningSecret,
                             app.Domain.RS256Params,
                             app.IsDomainManagement)
                }
                |> toSingleOptionAsync

            let result =
                match result with
                | Some (signingAlgorithmType, hs256SigningSecret, rs256Params, isDomainManagement) ->
                    match isDomainManagement with
                    | true -> HS256 jwtConfig.AccessTokenSecret
                    | false ->
                        match signingAlgorithmType with
                        | SigningAlgorithmType.HS256 -> HS256 hs256SigningSecret
                        | SigningAlgorithmType.RS256 -> RS256 rs256Params
                        | _ -> raise (unexpected "Unexpected signing algorithm type")
                | None -> raise (unAuthorized "Domain is not found for provided issuer")

            return result
        }

    let logout: LogOut =
        fun env data ->
            let logger = env.Logger
            let accessToken = data.AccessToken

            logger.LogInformation("logout with access token ${token}", accessToken)

            task {
                let clientId =
                    readToken accessToken
                    |> Option.bind (fun jwt -> jwt.Claims |> tryBindClaimClientId)

                let clientId =
                    match clientId with
                    | Some clientId ->
                        logger.LogInformation("Issuer found ${clientId}", clientId)
                        clientId
                    | None ->
                        logger.LogInformation("access_token is not valid or clientId not found")
                        raise (unAuthorized "access_token is not valid or clientId not found")

                let! secret = getSigningParams env.DataContext env.JwtConfig clientId

                logger.LogInformation("secret info ${@secret}", secret)

                let securityKey =
                    match secret with
                    | RS256 key -> createRS256Key key :> SecurityKey
                    | HS256 key -> createHS256Key key :> SecurityKey

                let tokenValidationParameters =
                    TokenValidationParameters
                        (ValidateAudience = false,
                         ValidateIssuer = false,
                         ValidateIssuerSigningKey = true,
                         IssuerSigningKey = securityKey,
                         ValidateLifetime = false)

                let principal =
                    validateToken accessToken tokenValidationParameters

                let principal =
                    match principal with
                    | Some principal -> principal
                    | None -> raise (unAuthorized "access_token is not valid")

                let claims = principalClaims principal

                let sub =
                    claims
                    |> getClaimInt CLAIM_TYPE_UID
                    |> noneFails (unAuthorized "sub is not found")

                let result = { ReturnUri = data.ReturnUri }

                let! _ = env.KeyValueStorage.RemoveValuesByTag<SSOKV> (sub.ToString()) None

                let! _ = env.KeyValueStorage.RemoveValuesByTag<RefreshTokenKV> (sub.ToString()) None

                return result
            }
