namespace PRR.Domain.Auth.SignUpConfirm

open Common.Domain.Models
open Common.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open Models
open PRR.Data.Entities
open PRR.Domain.Auth.Common
open System
open Microsoft.Extensions.Logging
open DataAvail.EntityFramework.Common

[<AutoOpen>]
module SignUpConfirm =

    let signUpConfirm: SignUpConfirm =
        fun env data ->

            env.Logger.LogInformation("Signup confirm")

            let token = data.Token

            task {

                let! item = env.KeyValueStorage.GetValue<SignUpKV> token None

                let item =
                    match item with
                    | Ok item ->
                        env.Logger.LogInformation("Signup item found ${@item}", { item with Password = "***" })
                        item
                    | Error err ->
                        env.Logger.LogWarning("Couldn't find signup item ${token} with error ${@error}", token, err)
                        raise (UnAuthorized None)

                if item.ExpiredAt < DateTime.UtcNow then
                    env.Logger.LogWarning("Signup item expired for token ${token}", token)
                    raise (UnAuthorized None)

                let dataContext = env.DataContext

                let user =
                    User
                        (FirstName = item.FirstName,
                         LastName = item.LastName,
                         Email = item.Email,
                         Password = item.Password)
                    |> add' dataContext

                do! saveChangesAsync dataContext


                env.Logger.LogInformation("Signup confirm success email {@email}", item.Email)

                do! env.KeyValueStorage.RemoveValuesByTag<SignUpKV> user.Email None

                return user.Id
            }
