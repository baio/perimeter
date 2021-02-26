namespace PRR.Domain.Auth.LogIn.Common

open PRR.Domain.Models
open PRR.Data.DataContext
open System.Linq
open DataAvail.EntityFramework.Common

[<AutoOpen>]
module internal UserHelpers =    
    

    type RolePermissions =
        { Role: string
          Permissions: string seq }

    let getUserDomainRolesPermissions (dataContext: DbDataContext) (domainId, userEmail) =
        query {
            for dur in dataContext.DomainUserRole do
                where
                    (dur.DomainId = domainId
                     && dur.UserEmail = userEmail)
                select
                    { Role = dur.Role.Name
                      Permissions = dur.Role.RolesPermissions.Select(fun x -> x.Permission.Name) }
        }
        |> toListAsync

    type DomainAudiences =
        { DomainId: int
          Audiences: string seq }

    let getClientDomainAudiences (dataContext: DbDataContext) clientId =
        query {
            for app in dataContext.Applications do
                where (app.ClientId = clientId)
                select
                    { DomainId = app.Domain.Id
                      Audiences = app.Domain.Apis.Select(fun x -> x.Identifier) }
        }
        |> toSingleOptionAsync
