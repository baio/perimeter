namespace PRR.Domain.Auth.RefreshToken

open PRR.Domain.Models
open Microsoft.IdentityModel.Tokens
open System
open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open System.Text
open PRR.Domain.Auth.Common

[<AutoOpen>]
module internal ValidateAccessToken =

    open DataAvail.Common
    open DataAvail.Common.Option

    let getClaim' f claimType (claims: Claim seq) =
        claims
        |> Seq.tryFind (fun x -> x.Type = claimType)
        >>= fun claim -> claim.Value |> f

    let getClaim x = x |> getClaim' Some

    let getClaimInt x = x |> getClaim' tryParseInt

    let validateAccessToken (token: Token) (key: SecurityKey) =

        let tokenValidationParameters =
            TokenValidationParameters
                (ValidateAudience = false,
                 ValidateIssuer = false,
                 ValidateIssuerSigningKey = true,
                 IssuerSigningKey = key,
                 ValidateLifetime = false)

        (principalClaims
         <!> validateToken token tokenValidationParameters)
        >>= getClaimInt CLAIM_TYPE_UID

    let getTokenIssuer =
        readToken >> Option.map (fun x -> x.Issuer)
