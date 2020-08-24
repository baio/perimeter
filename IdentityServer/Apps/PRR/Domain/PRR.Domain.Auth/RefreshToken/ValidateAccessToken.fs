namespace PRR.Domain.Auth.RefreshToken

open Common.Domain.Models
open Common.Utils
open Microsoft.IdentityModel.Tokens
open System
open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open System.Text

[<AutoOpen>]
module internal ValidateAccessToken =

    let validateToken (token: string) tokenValidationParameters =
        let tokenHandler = JwtSecurityTokenHandler()
        try
            let (principal, securityToken) = tokenHandler.ValidateToken(token, tokenValidationParameters)
            let jwtSecurityToken = securityToken :?> JwtSecurityToken
            if (jwtSecurityToken = null
                || (jwtSecurityToken.Header.Alg.Equals
                        (SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase) |> not)) then None
            else Some principal
        with :? System.ArgumentException -> None

    let private principalCalims (principal: ClaimsPrincipal) = principal.Claims

    open FSharpx.Option

    let getClaim' f claimType (claims: Claim seq) =
        claims
        |> Seq.tryFind (fun x -> x.Type = claimType)
        >>= fun claim -> claim.Value |> f

    let getClaim x = x |> getClaim' Some

    let getClaimInt x = x |> getClaim' tryParseInt

    let validateAccessToken (token: Token) (key: string) =
        let tokenValidationParameters =
            TokenValidationParameters
                (ValidateAudience = false, ValidateIssuer = false, ValidateIssuerSigningKey = true,
                 IssuerSigningKey = SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), ValidateLifetime = false)

        (principalCalims <!> validateToken token tokenValidationParameters) >>= getClaimInt ClaimTypes.NameIdentifier
