namespace PRR.Domain.Auth.Common

open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open System.Security.Cryptography
open Microsoft.IdentityModel.Tokens

[<AutoOpen>]
module Security =

    let private createSymmetricKey (secret: string) =
        secret
        |> System.Text.Encoding.ASCII.GetBytes
        |> SymmetricSecurityKey

    let createHS256Key (secret: string) = createSymmetricKey secret

    let createRS256Key (xmlParams: string) =
        let rsa = RSA.Create()
        rsa.FromXmlString(xmlParams)
        RsaSecurityKey(rsa.ExportParameters(true))
        
    let principalClaims (principal: ClaimsPrincipal) = principal.Claims        

    let validateToken (token: string) tokenValidationParameters =
        let tokenHandler = JwtSecurityTokenHandler()

        try
            let (principal, securityToken) =
                tokenHandler.ValidateToken(token, tokenValidationParameters)

            let jwtSecurityToken = securityToken :?> JwtSecurityToken
            if (jwtSecurityToken = null) then None else Some principal
        with ex ->
            printfn "Validate token fails %O" ex
            None
            
    let readToken (token: string) =
        let tokenHandler = JwtSecurityTokenHandler()
        try
            let securityToken = tokenHandler.ReadJwtToken(token)
            if (securityToken = null) then None else Some securityToken
        with ex ->
            printfn "Read token fails %O" ex
            None
            
    //
    let CLAIM_TYPE_CID = "cid"            
    
    let claimValue (claim: Claim) = claim.Value
    let claimType (claim: Claim) = claim.Type

    let tryBindClaim' fn =        
        Seq.tryFind (claimType >> fn)
        >> Option.map claimValue

    let tryBindClaim name = tryBindClaim' (fun x -> x = name)
   
    let tryBindClaimClientId x = x |> tryBindClaim CLAIM_TYPE_CID
