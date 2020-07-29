namespace PRR.Domain.Auth.ResetPasswordConfirm

open Common.Domain.Models
open Common.Domain.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open PRR.Data.Entities
open PRR.Domain.Auth.ResetPasswordConfirm.Models
open PRR.Domain.Auth.Utils
open PRR.System.Models
open System

[<AutoOpen>]
module ResetPasswordConfirm =

    let validateData (data: Data) =
        Utils.validatePassword data.Password |> Array.choose id

    let resetPasswordConfirm: ResetPasswordConfirm =
        fun env item data ->

            if item.ExpiredAt < DateTime.UtcNow then raise UnAuthorized

            task {
                let saltedPassword = env.PasswordSalter data.Password
                do! updateSingleRawAsyncExn UnAuthorized env.DataContext.Users {| Password = saltedPassword |}
                        {| Email = item.Email |}
                return item.Email |> ResetPasswordUpdated
            }
