namespace PRR.Domain.Auth.UpdatePassword

open Common.Domain.Models
open Common.Domain.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext

[<AutoOpen>]
module UpdatePassword =

    let updatePassword: UpdatePassword =
        fun env (userId, data) ->
            let oldSaltedPassword = env.PasswordSalter data.OldPassword
            let saltedPassword = env.PasswordSalter data.Password
            updateSingleRawAsyncExn UnAuthorized env.DataContext.Users {| Password = saltedPassword |}
                {| Id = userId
                   Password = oldSaltedPassword |}
