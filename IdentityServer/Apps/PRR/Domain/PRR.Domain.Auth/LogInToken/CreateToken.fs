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
        let issuedAt = DateTime.Now
        let expires = issuedAt.AddMinutes(float expireInMinutes)
        let signingCredentials = SigningCredentials(SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        SecurityTokenDescriptor
            (Subject = subject, Expires = Nullable(expires), SigningCredentials = signingCredentials,
             IssuedAt = Nullable(issuedAt))
        |> tokenHandler.CreateToken
        |> tokenHandler.WriteToken
