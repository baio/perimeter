namespace PRR.Domain.Auth.SignUpConfirm

open Common.Domain.Models
open Common.Domain.Utils.LinqHelpers
open Common.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open Models
open PRR.Data.Entities
open PRR.System.Models
open System

[<AutoOpen>]
module SignUpConfirm =

    let signUpConfirm: SignUpConfirm =
        fun env item ->

            if item.ExpiredAt < DateTime.UtcNow then raise UnAuthorized

            let dataContext = env.DataContext

            task {
                let user =
                    User
                        (FirstName = item.FirstName, LastName = item.LastName, Email = item.Email,
                         Password = item.Password) |> add' dataContext
                do! saveChangesAsync dataContext
                return { UserId = user.Id
                         Email = user.Email }
                       |> UserSignUpConfirmedEvent
            }
