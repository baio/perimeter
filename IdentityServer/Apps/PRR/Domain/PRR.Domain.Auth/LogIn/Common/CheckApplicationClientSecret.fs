namespace PRR.Domain.Auth.LogIn.Common

open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open DataAvail.EntityFramework.Common.LinqHelpers
open DataAvail.Http.Exceptions

[<AutoOpen>]
module CheckApplicationClientSecret =

    let checkApplicationClientSecret (dbContext: DbDataContext) clientId clientSecret =

        task {

            let! appClientSecret =
                query {
                    for app in dbContext.Applications do
                        where (app.ClientId = clientId)
                        select app.ClientSecret
                }
                |> toSingleOptionAsync

            match appClientSecret with
            | None -> return Some(unAuthorized "Application client secret is not found")
            | Some appClientSecret ->
                if clientSecret <> appClientSecret
                then return Some(unAuthorized "Client secret mismatch")
                else return None
        }
