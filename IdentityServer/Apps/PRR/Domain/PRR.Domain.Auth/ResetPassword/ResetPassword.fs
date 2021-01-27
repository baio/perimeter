namespace PRR.Domain.Auth.ResetPassword

open Common.Domain.Models
open Common.Domain.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open HttpFs.Logging
open PRR.Domain.Auth.ResetPassword.Models
open Microsoft.Extensions.Logging
open System

[<AutoOpen>]
module ResetPassword =

    let resetPassword: ResetPassword =
        fun env data ->
            env.Logger.LogInformation("Reset password ${@data}", data)
            task {
                let! cnt =
                    query {
                        for user in env.DataContext.Users do
                            where (user.Email = data.Email)
                            select user.Id
                    }
                    |> toCountAsync

                if cnt = 0 then
                    env.Logger.LogWarning("User ${email} is not found", data.Email)
                    raise NotFound
                else
                    let expiredAt =
                        DateTime.UtcNow.AddMinutes(float (int env.TokenExpiresIn))
#if E2E
                    let token = "HASH"
#else
                    let token = env.HashProvider()
#endif
                    env.Logger.LogInformation
                        ("On success for email ${email} token expires at ${@expiredAt}", data.Email, expiredAt)
                    do! onSuccess env data.Email token expiredAt
            }
