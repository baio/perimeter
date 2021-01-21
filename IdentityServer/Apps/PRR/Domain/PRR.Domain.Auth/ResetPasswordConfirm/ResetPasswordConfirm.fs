namespace PRR.Domain.Auth.ResetPasswordConfirm

open Common.Domain.Models
open Common.Domain.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Domain.Auth.Common
open PRR.Domain.Auth.ResetPasswordConfirm.Models
open PRR.Domain.Auth.Utils
open PRR.System.Models
open System
open Microsoft.Extensions.Logging

[<AutoOpen>]
module ResetPasswordConfirm =

    let private validateData (data: Data) =
        validatePassword data.Password |> mapBadRequest

    let resetPasswordConfirm: ResetPasswordConfirm =
        fun env data ->

            env.Logger.LogInformation("Reset password confirm")

            task {
                let! item = env.GetTokenItem data.Token

                let item =
                    match item with
                    | Some item ->
                        env.Logger.LogInformation("Reset password item found ${item}", { item with Token = "***" })
                        item
                    | None ->
                        env.Logger.LogWarning("Reset password item is not found for ${token}", data.Token)
                        raise UnAuthorized'

                if item.ExpiredAt < DateTime.UtcNow then
                    env.Logger.LogWarning("Reset password item expired for token ${token}", data.Token)
                    raise UnAuthorized'

                let saltedPassword = env.PasswordSalter data.Password

                do! updateSingleRawAsyncExn
                        (UnAuthorized None)
                        env.DataContext.Users
                        {| Password = saltedPassword |}
                        {| Email = item.Email |}

                env.Logger.LogInformation("Reset password confirm success")

                do! env.OnSuccess item.Email
            }
