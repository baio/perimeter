namespace PRR.Domain.Auth.LogInToken

open Common.Domain.Models
open Microsoft.IdentityModel.Tokens
open System
open System.IdentityModel.Tokens.Jwt
open System.Security.Claims
open System.Text

[<AutoOpen>]
module private CreateAccessToken =

    let createToken (secret: string) (expireInMinutes: int<minutes>) (claims: Claim seq)  =
        let subject = claims |> ClaimsIdentity
        let tokenHandler = JwtSecurityTokenHandler()
        let key = Encoding.ASCII.GetBytes secret
        let expires = System.Nullable(DateTime.UtcNow.AddMinutes(float expireInMinutes))
        let signingCredentials = SigningCredentials(SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        SecurityTokenDescriptor
            (Subject = subject, Expires = expires, SigningCredentials = signingCredentials)
        |> tokenHandler.CreateToken
        |> tokenHandler.WriteToken
