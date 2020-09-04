namespace PRR.Domain.Auth

open System
open System
open Common.Utils
open PRR.Data.DataContext
open Common.Domain.Utils
open Common.Domain.Models
open FSharp.Control.Tasks.V2
open FSharp.Control.Tasks.V2.ContextInsensitive

[<AutoOpen>]
module ValidateScopes =

    let private getTenantManagementAudiencePermissions (dataContext: DbDataContext) domainId =
        task {

            let! aud =
                query {
                    for api in dataContext.Apis do
                        where (api.DomainId = domainId)
                        select api.Identifier
                }
                |> toSingleAsync

            let! perms =
                query {
                    for perm in dataContext.Permissions do
                        where (perm.IsTenantManagement = true)
                        select perm.Name
                }
                |> toListAsync

            return seq { { Audience = aud; Scopes = perms } }
        }

    let private getDomainManagementAudiencePermissions (dataContext: DbDataContext) domainId =
        task {
            let! aud =
                query {
                    for api in dataContext.Apis do
                        where
                            (api.DomainId = domainId
                             && api.IsDomainManagement = true)
                        select api.Identifier
                }
                |> toSingleAsync

            let! perms =
                query {
                    for perm in dataContext.Permissions do
                        where (perm.IsDomainManagement = true)
                        select perm.Name
                }
                |> toListAsync


            return seq { { Audience = aud; Scopes = perms } }
        }

    let private getDomainAudiencePermissions (dataContext: DbDataContext) userEmail domainId scopes =
        query {
            for dur in dataContext.DomainUserRole do
                join rp in dataContext.RolesPermissions on (dur.RoleId = rp.RoleId)
                where
                    (dur.UserEmail = userEmail
                     && dur.DomainId = domainId
                     && (%in' scopes) rp.Permission.Name)
                select (Tuple.Create(rp.Permission.Api.Identifier, rp.Permission.Name))
        }
        |> groupByAsync (fun (aud, perms) -> { Audience = aud; Scopes = perms })                               


    let validateScopes (dataContext: DbDataContext) userEmail clientId scopes =
        task {

            let! (domainId, isDomainManagement, isTenantManagement) =
                query {
                    for app in dataContext.Applications do
                        where (app.ClientId = clientId)
                        select (app.DomainId, app.IsDomainManagement, app.Domain.Tenant <> null)
                }
                |> toSingleAsync

            if isTenantManagement
            then return! getTenantManagementAudiencePermissions dataContext domainId
            else if isDomainManagement
            then return! getDomainManagementAudiencePermissions dataContext domainId
            else return! getDomainAudiencePermissions dataContext userEmail domainId scopes
        }
