namespace PRR.Domain.Auth.LogIn.Common

open PRR.Domain.Common.Events
open PRR.Domain.Models

[<AutoOpen>]
module GetLogInEvent =

    open FSharp.Control.Tasks.V2.ContextInsensitive
    open PRR.Data.DataContext
    open System
    open PRR.Domain.Auth.Common
    open PRR.Domain.Common
    open DataAvail.EntityFramework.Common
    open DataAvail.Http.Exceptions


    let getLoginEvent (dataContext: DbDataContext) clientId (grantType: LogInGrantType) isPerimeterClient =
        task {

            if clientId = PERIMETER_CLIENT_ID then
                let result: Events.LogInEvent =
                    { DomainId = PERIMETER_DOMAIN_ID
                      IsManagementClient = true
                      AppIdentifier = PERIMETER_APP_IDENTIFIER
                      ClientId = clientId
                      DateTime = DateTime.UtcNow
                      GrantType = grantType }

                return result
            else
                let! (domainId, appIdentifier) =
                    query {
                        for app in dataContext.Applications do
                            where (app.ClientId = clientId)
                            select (app.Domain.Id, app.Name)
                    }
                    |> toSingleExnAsync (Unexpected')

                let successData: Events.LogInEvent =
                    { DomainId = domainId
                      IsManagementClient = isPerimeterClient
                      AppIdentifier = appIdentifier
                      ClientId = clientId
                      DateTime = DateTime.UtcNow
                      GrantType = grantType }

                return successData
        }
