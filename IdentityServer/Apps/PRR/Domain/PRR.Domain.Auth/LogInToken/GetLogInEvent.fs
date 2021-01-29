namespace PRR.Domain.Auth.LogInToken

open PRR.Domain.Models
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open System
open PRR.Domain.Auth.Common
open PRR.Domain.Common
open DataAvail.EntityFramework.Common
open DataAvail.Http.Exceptions

[<AutoOpen>]
module private GetLogInEvent =

    let getLoginEvent (dataContext: DbDataContext) clientId userId social isPerimeterClient =
        task {

            let! userEmail =
                query {
                    for user in dataContext.Users do
                        where (user.Id = userId)
                        select user.Email
                }
                |> toSingleExnAsync (Unexpected')

            if clientId = PERIMETER_CLIENT_ID then
                let result: Events.LogIn =
                    { DomainId = PERIMETER_DOMAIN_ID
                      Social = social
                      IsManagementClient = true
                      AppIdentifier = PERIMETER_APP_IDENTIFIER
                      UserId = userId
                      UserEmail = userEmail
                      ClientId = clientId
                      DateTime = DateTime.UtcNow }

                return result
            else
                let! (domainId, appIdentifier) =
                    query {
                        for app in dataContext.Applications do
                            where (app.ClientId = clientId)
                            select (app.Domain.Id, app.Name)
                    }
                    |> toSingleExnAsync (Unexpected')

                let successData: Events.LogIn =
                    { DomainId = domainId
                      IsManagementClient = isPerimeterClient
                      AppIdentifier = appIdentifier
                      UserId = userId
                      Social = social
                      UserEmail = userEmail
                      ClientId = clientId
                      DateTime = DateTime.UtcNow }

                return successData
        }
