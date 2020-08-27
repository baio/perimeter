namespace PRR.Domain.Auth.LogInToken

open Common.Domain.Models
open Microsoft.IdentityModel.Tokens
open System
open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open System.Text

[<AutoOpen>]
module private CreateAccessToken =

    let createToken (secret: string) (expireInMinutes: int<minutes>) (claims: Claim seq) =
        let subject = claims |> ClaimsIdentity
        let tokenHandler = JwtSecurityTokenHandler()
        let key = Encoding.ASCII.GetBytes secret
        let issuedAt = DateTime.UtcNow
        let expires = issuedAt.AddMinutes(float expireInMinutes)
        let notBefore = expires.AddMinutes(float -1)
        let signingCredentials = SigningCredentials(SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        SecurityTokenDescriptor
            (Subject = subject, IssuedAt = Nullable(issuedAt), Expires = Nullable(expires),
             NotBefore = Nullable(notBefore), SigningCredentials = signingCredentials)
        |> tokenHandler.CreateToken
        |> tokenHandler.WriteToken
