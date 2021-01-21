namespace PRR.Domain.Auth.SignUpConfirm

open Common.Domain.Models
open Common.Domain.Utils.LinqHelpers
open Common.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open Models
open PRR.Data.Entities
open PRR.System.Models
open System
open Microsoft.Extensions.Logging

[<AutoOpen>]
module SignUpConfirm =

    let signUpConfirm: SignUpConfirm =
        fun env data ->

            env.Logger.LogInformation("Signup confirm")

            let token = data.Token

            task {

                let! item = env.GetTokenItem token

                let item =
                    match item with
                    | Some item ->
                        env.Logger.LogInformation("Signup item found ${@item}", { item with Password = "***" })
                        item
                    | None ->
                        env.Logger.LogWarning("Couldn't find signup item ${token}", token)
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

                let successData = { UserId = user.Id; Email = user.Email }

                env.Logger.LogInformation("Signup confirm success data {@data}", successData)


                env.OnSuccess successData
            }
