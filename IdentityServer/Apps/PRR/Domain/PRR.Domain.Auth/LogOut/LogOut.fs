﻿namespace PRR.Domain.Auth.LogOut

open Common.Domain.Models
open Common.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.IdentityModel.Tokens
open PRR.Domain.Auth.RefreshToken
open PRR.System.Models
open System.Security.Claims
open System.Text
open System.Threading.Tasks

[<AutoOpen>]
module LogOut =

    let logout: LogOut =
        fun env data ->
            let accessToken = data.AccessToken
            let tokenValidationParameters =
                TokenValidationParameters
                    (ValidateAudience = false, ValidateIssuer = false, ValidateIssuerSigningKey = true,
                     IssuerSigningKey = SymmetricSecurityKey(Encoding.UTF8.GetBytes(env.AccessTokenSecret)),
                     ValidateLifetime = false)
            let token = validateToken accessToken tokenValidationParameters
            match token with
            | Some token ->
                // TODO : Check allowed return url
                let sub =
                    token.Claims
                    |> getClaimInt CLAIM_TYPE_UID
                    |> Options.noneFails (unAuthorized "sub is not found")

                let result = { ReturnUri = data.ReturnUri }
                task {
                    do! env.OnSuccess sub
                    return result
                }
            | None ->
                raise (Exceptions.unAuthorized "access_token is not valid")
