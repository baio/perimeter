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
                let! item = env.KeyValueStorage.GetValue<ResetPasswordKV> data.Token None

                let item =
                    match item with
                    | Ok item ->
                        env.Logger.LogInformation("Reset password item found ${item}", item)
                        item
                    | Error err ->
                        env.Logger.LogWarning
                            ("Reset password item is not found for ${token} with ${@error}", data.Token, err)
                        raise UnAuthorized'


                let saltedPassword = env.PasswordSalter data.Password

                do! updateSingleRawAsyncExn
                        (UnAuthorized None)
                        env.DataContext.Users
                        {| Password = saltedPassword |}
                        {| Email = item.Email |}

                env.Logger.LogInformation("Reset password confirm success")

                do! env.KeyValueStorage.RemoveValuesByTag<ResetPasswordKV> item.Email None
            }
