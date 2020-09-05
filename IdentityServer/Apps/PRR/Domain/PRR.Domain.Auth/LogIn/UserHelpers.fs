namespace PRR.Domain.Auth.LogIn

open Common.Domain.Models
open Common.Domain.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open PRR.Domain.Auth

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


    let getClientIdAndIssuer (dataContext: DbDataContext) clientId email =
        task {
            if clientId = DEFAULT_CLIENT_ID then
                let! res =
                    query {
                        for app in dataContext.Applications do
                            where (app.Domain.Tenant.User.Email = email)
                            select (app.ClientId, app.Domain.Issuer)
                    }
                    |> LinqHelpers.toSingleOptionAsync

                return match res with
                       | Some res -> res
                       | None -> (PERIMETER_CLIENT_ID, PERIMETER_ISSUER)
            else
                let! issuer =
                    query {
                        for app in dataContext.Applications do
                            where (app.ClientId = clientId)
                            select app.Domain.Issuer
                    }
                    |> LinqHelpers.toSingleExnAsync (unexpected (sprintf "ClientId %s is not found" clientId))                
                return (clientId, issuer)
        }
