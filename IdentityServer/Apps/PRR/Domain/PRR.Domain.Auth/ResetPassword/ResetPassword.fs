namespace PRR.Domain.Auth.ResetPassword

open Common.Domain.Models
open Common.Domain.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Domain.Auth.ResetPassword.Models
open Microsoft.Extensions.Logging

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
                    env.Logger.LogWarning("User ${email} found successfully", data.Email)
                    env.OnSuccess data.Email
            }
