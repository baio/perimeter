namespace PRR.Domain.Auth.SignUpConfirm

open System.Threading.Tasks
open DataAvail.Common
open PRR.Domain.Models
open FSharp.Control.Tasks.V2.ContextInsensitive
open Models
open PRR.Data.Entities
open PRR.Domain.Auth.Common
open System
open Microsoft.Extensions.Logging
open DataAvail.EntityFramework.Common
open DataAvail.Http.Exceptions

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
                        env.Logger.LogDebug("Signup item found ${@item}", { item with Password = "***" })
                        item
                    | Error err ->
                        env.Logger.LogWarning("Couldn't find signup item ${token} with error ${@error}", token, err)
                        raise (UnAuthorized None)

                if item.ExpiredAt < DateTime.UtcNow then
                    env.Logger.LogWarning("Signup item expired for token ${token}", token)
                    raise (UnAuthorized None)

                let dataContext = env.DataContext

                let existentUserId = item.ExistentUserId


                let! userId =
                    match existentUserId with
                    | Some userId ->
                        env.Logger.LogDebug("UserId {userId} is found in the stored item, just update password", userId)

                        task {
                            update dataContext (fun (user: User) -> user.Password <- item.Password) (User(Id = userId))
                            do! saveChangesAsync dataContext
                            return userId
                        }
                    | None ->
                        env.Logger.LogDebug("UserId is not found in the stored item, create new user")

                        task {
                            let user =
                                User
                                    (FirstName = item.FirstName,
                                     LastName = item.LastName,
                                     Email = item.Email,
                                     Password = item.Password)
                                |> add' dataContext

                            do! saveChangesAsync dataContext
                            return user.Id
                        }


                env.Logger.LogInformation("Signup confirm success email {@email}", item.Email)

                do! env.KeyValueStorage.RemoveValuesByTag<SignUpKV> item.Email None

                return userId
            }
