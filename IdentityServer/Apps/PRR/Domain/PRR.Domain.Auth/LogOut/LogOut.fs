namespace PRR.Domain.Auth.LogOut

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

[<AutoOpen>]
module LogOut =

    let logout: LogOut =
        fun env data ->
            let accessToken = data.AccessToken

            let tokenValidationParameters =
                TokenValidationParameters
                    (ValidateAudience = false,
                     ValidateIssuer = false,
                     ValidateIssuerSigningKey = true,
                     IssuerSigningKey = SymmetricSecurityKey(Encoding.UTF8.GetBytes(env.AccessTokenSecret)),
                     ValidateLifetime = false)

            let token =
                validateToken accessToken tokenValidationParameters

            match token with
            | Some token ->
                // TODO : Check allowed return url
                let sub =
                    token.Claims
                    |> getClaimInt CLAIM_TYPE_UID
                    |> noneFails (unAuthorized "sub is not found")

                let result = { ReturnUri = data.ReturnUri }
                task {
                    let! _ = env.KeyValueStorage.RemoveValuesByTag<SSOKV> (sub.ToString()) None

                    let! _ = env.KeyValueStorage.RemoveValuesByTag<RefreshTokenKV> (sub.ToString()) None

                    return result
                }
            | None -> raise (Exceptions.unAuthorized "access_token is not valid")
