namespace PRR.Domain.Auth.LogIn.Common

open PRR.Domain.Models
open Microsoft.IdentityModel.Tokens
open System
open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open System.Text

[<AutoOpen>]
module private CreateToken =

    let createSignedToken (signingCredentials: SigningCredentials) (expireInMinutes: int<minutes>) (claims: Claim seq) =
        let subject = claims |> ClaimsIdentity
        let tokenHandler = JwtSecurityTokenHandler()
        // let key = Encoding.ASCII.GetBytes secret
        let issuedAt = DateTime.UtcNow
        // TODO : Not before will be added automatically and could potentially fails if user PC time is wrong
        // Should be able set skew on client ?
        let issuedAtSkew = issuedAt.AddMinutes(float -1)

        let expires =
            issuedAt.AddMinutes(float expireInMinutes)

        SecurityTokenDescriptor
            (Subject = subject,
             Expires = Nullable(expires),
             SigningCredentials = signingCredentials,
             IssuedAt = Nullable(issuedAt),
             NotBefore = Nullable(issuedAtSkew))
        |> tokenHandler.CreateToken
        |> tokenHandler.WriteToken


    let createUnsignedToken (expireInMinutes: int<minutes>) (claims: Claim seq) =
        let subject = claims |> ClaimsIdentity
        let tokenHandler = JwtSecurityTokenHandler()
        let issuedAt = DateTime.Now
        let issuedAtSkew = issuedAt.AddMinutes(float -1)

        let expires =
            issuedAt.AddMinutes(float expireInMinutes)

        SecurityTokenDescriptor
            (Subject = subject,
             Expires = Nullable(expires),
             IssuedAt = Nullable(issuedAt),
             NotBefore = Nullable(issuedAtSkew))
        |> tokenHandler.CreateToken
        |> tokenHandler.WriteToken
