namespace PRR.Domain.Auth.ResetPassword

open Common.Domain.Models
open Common.Domain.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open HttpFs.Logging
open PRR.Domain.Auth.ResetPassword.Models
open Microsoft.Extensions.Logging
open PRR.System.Models
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
                    let successData: ResetPassword.Item =
                        {
                          ExpiredAt = DateTime.UtcNow.AddMinutes(float (int env.TokenExpiresIn)) 
                          Email = data.Email
#if E2E
                          Token = "HASH"
#else
                          Token = env.HashProvider()
#endif
                        }
                    
                    env.Logger.LogWarning("${@successData} ready", {successData with Token = "***"})
                    env.OnSuccess successData
            }
