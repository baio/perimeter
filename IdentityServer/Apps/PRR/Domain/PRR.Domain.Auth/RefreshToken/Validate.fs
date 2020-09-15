namespace PRR.Domain.Auth.RefreshToken

open Common.Domain.Models
open System
open System.Threading.Tasks
open PRR.Domain.Auth.LogInToken

[<AutoOpen>]
module private Validate =

    type ValidateResult =
        | AccessTokenInvalid
        | RefreshTokenNotFound
        | RefreshTokenExpired
        | UserNotMatch
        | Success

    let validate env accessToken (expiresAt, userId, secret) =
        match expiresAt < DateTime.UtcNow with
        | true -> RefreshTokenExpired
        | false ->
            match validateAccessToken accessToken secret with
            | Some userId' -> if userId <> userId' then UserNotMatch else Success
            | None -> AccessTokenInvalid
