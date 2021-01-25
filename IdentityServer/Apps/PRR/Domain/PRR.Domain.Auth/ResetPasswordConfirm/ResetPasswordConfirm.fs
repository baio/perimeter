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
                let! tokenEmail = env.GetTokenItem data.Token

                let tokenEmail =
                    match tokenEmail with
                    | Some item ->
                        env.Logger.LogInformation("Reset password item found ${item}", item)
                        item
                    | None ->
                        env.Logger.LogWarning("Reset password item is not found for ${token}", data.Token)
                        raise UnAuthorized'


                let saltedPassword = env.PasswordSalter data.Password

                do! updateSingleRawAsyncExn
                        (UnAuthorized None)
                        env.DataContext.Users
                        {| Password = saltedPassword |}
                        {| Email = tokenEmail |}

                env.Logger.LogInformation("Reset password confirm success")

                do! env.OnSuccess tokenEmail
            }
