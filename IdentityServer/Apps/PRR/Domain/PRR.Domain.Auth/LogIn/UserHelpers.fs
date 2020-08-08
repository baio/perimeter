namespace PRR.Domain.Auth.LogIn

open Common.Domain.Models
open Common.Domain.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext

[<AutoOpen>]
module internal UserHelpers =
    
    let private DEFAULT_CLIENT_ID = "__DEFAULT_CLIENT_ID__"

    let getUserId (dataContext: DbDataContext) (email, password) =
        query {
            for user in dataContext.Users do
                where (user.Email = email && user.Password = password)
                select user.Id
        }
        |> toSingleOptionAsync


    let getClientId (dataContext: DbDataContext) clientId email =
        task {
            if clientId = DEFAULT_CLIENT_ID then
                return! query {
                            for app in dataContext.Applications do
                                where (app.Domain.Tenant.User.Email = email)
                                select app.ClientId
                        }
                        |> LinqHelpers.toSingleExnAsync (unAuthorized "Tenant's management API is not found")
            else
                return clientId
        }
