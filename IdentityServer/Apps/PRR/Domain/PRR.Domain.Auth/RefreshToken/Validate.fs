namespace PRR.Domain.Auth.RefreshToken

open Common.Domain.Models
open System
open System.Threading.Tasks

[<AutoOpen>]
module private Validate =

    type ValidateResult =
        | AccessTokenInvalid
        | RefreshTokenNotFound
        | RefreshTokenExpired
        | UserNotMatch
        | Success

    let validate env accessToken (expiresAt, userId) =
        match expiresAt < DateTime.UtcNow with
        | true -> RefreshTokenExpired
        | false ->
            match validateAccessToken accessToken env.JwtConfig.AccessTokenSecret with
            | Some userId' ->
                if userId <> userId' then UserNotMatch
                else Success
            | None -> AccessTokenInvalid
