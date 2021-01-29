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

    let validateToken (token: string) tokenValidationParameters =
        let tokenHandler = JwtSecurityTokenHandler()
        try
            let (principal, securityToken) =
                tokenHandler.ValidateToken(token, tokenValidationParameters)

            let jwtSecurityToken = securityToken :?> JwtSecurityToken
            if (jwtSecurityToken = null) then
                (*                || (jwtSecurityToken.Header.Alg.Equals
                        (SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase)
                    |> not)) then
                *)
                None
            else
                Some principal
        with :? Exception as ex ->
            printfn "Validate token fails %O" ex
            None


    let private principalClaims (principal: ClaimsPrincipal) = principal.Claims

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
                 IssuerSigningKey = key,  //SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                 ValidateLifetime = false)

        (principalClaims
         <!> validateToken token tokenValidationParameters)
        >>= getClaimInt CLAIM_TYPE_UID


    let getTokenIssuer =
        readToken >> Option.map (fun x -> x.Issuer)
