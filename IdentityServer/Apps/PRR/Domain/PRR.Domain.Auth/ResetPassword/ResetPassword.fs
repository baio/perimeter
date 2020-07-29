namespace PRR.Domain.Auth.ResetPassword

open Common.Domain.Models
open Common.Domain.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Domain.Auth.ResetPassword.Models
open PRR.System.Models

[<AutoOpen>]
module ResetPassword =

    let resetPassword: ResetPassword =
        fun env data ->
            task {
                let! cnt = query {
                               for user in env.Users do
                                   where (user.Email = data.Email)
                                   select user.Id
                           }
                           |> toCountAsync

                if cnt = 0 then return raise NotFound
                else return ResetPasswordRequested data.Email
            }
