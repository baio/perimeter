namespace PRR.Domain.Auth.UpdatePassword

open Common.Domain.Models
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open PRR.Domain.Auth.Utils
open DataAvail.EntityFramework.Common
open DataAvail.Http.Exceptions

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
