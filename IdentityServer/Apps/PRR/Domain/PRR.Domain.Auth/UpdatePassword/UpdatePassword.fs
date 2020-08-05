namespace PRR.Domain.Auth.UpdatePassword

open Common.Domain.Models
open Common.Domain.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open PRR.Domain.Auth.Utils

[<AutoOpen>]
module UpdatePassword =

    let validateData (data: Data) =
        validatePassword data.Password |> Array.choose id

    let updatePassword: UpdatePassword =
        fun env (userId, data) ->
            let oldSaltedPassword = env.PasswordSalter data.OldPassword
            let saltedPassword = env.PasswordSalter data.Password
            updateSingleRawAsyncExn (UnAuthorized None) env.DataContext.Users {| Password = saltedPassword |}
                {| Id = userId
                   Password = oldSaltedPassword |}
