namespace PRR.Domain.Auth.LogIn.RefreshToken

open PRR.Domain.Models
open Microsoft.IdentityModel.Tokens
open PRR.Domain.Auth.Common

[<AutoOpen>]
module internal ValidateAccessToken =

    open DataAvail.Common
    open DataAvail.Common.Option

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
