namespace PRR.Domain.Auth.Common

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

    type private AppDomainType =
        | TenantManagement
        | DomainManagement
        | Generic

    let private getTenantManagementAudience (dataContext: DbDataContext) domainId =
        query {
            for api in dataContext.Apis do
                where (api.DomainId = domainId)
                select api.Identifier
        }
        |> toSingleAsync

    let private getDomainManagementAudience (dataContext: DbDataContext) domainId =
        query {
            for api in dataContext.Apis do
                where
                    (api.DomainId = domainId
                     && api.IsDomainManagement = true)
                select api.Identifier
        }
        |> toSingleAsync

    /// For domain and tenant management apis returns audience and shared permissions (scopes)
    let private getManagementAudience (dataContext: DbDataContext) domainId =
        function
        | TenantManagement -> getTenantManagementAudience dataContext domainId
        | DomainManagement -> getDomainManagementAudience dataContext domainId

    let private userHasAssignedRole (dataContext: DbDataContext) userEmail domainId =
        query {
            for dur in dataContext.DomainUserRole do
                where
                    (dur.UserEmail = userEmail
                     && dur.DomainId = domainId)
                select dur.RoleId
        }
        |> toCountAsync
        |> TaskUtils.map (fun i -> i > 0)

    let private getDefaultDomainAudiencePermissions (dataContext: DbDataContext) domainId scopes =
        printfn "77777777"
        query {
            for p in dataContext.Permissions do
                where
                    (p.IsDefault = true
                     && p.Api.DomainId = domainId
                     && (%in' scopes) p.Name)
                select (Tuple.Create(p.Api.Identifier, p.Name))
        }
        |> groupByAsync (fun (aud, perms) -> { Audience = aud; Scopes = perms })

    let private getGenericDomainAudiencePermissions (dataContext: DbDataContext) userEmail domainId scopes =
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

    let private getManagementDomainPermissions (dataContext: DbDataContext) userEmail domainId appDomainType =
        let isDomainManagement = appDomainType = DomainManagement
        let isTenantManagement = appDomainType = TenantManagement
        query {
            for dur in dataContext.DomainUserRole do
                join rp in dataContext.RolesPermissions on (dur.RoleId = rp.RoleId)
                where
                    (dur.UserEmail = userEmail
                     && dur.DomainId = domainId
                     && rp.Permission.IsDomainManagement = isDomainManagement
                     && rp.Permission.IsTenantManagement = isTenantManagement)
                select rp.Permission.Name
        }
        |> toListAsync

    let private getAppDomainType isTenantManagement isDomainManagement =
        match (isTenantManagement, isDomainManagement) with
        | (false, false) -> Generic
        | (true, true) -> TenantManagement
        | (false, true) -> DomainManagement
        | (true, false) -> raise (unexpected "TODO: Domain couldn't be both tenant and domain management")

    let private getAppDomainInfo (dataContext: DbDataContext) clientId =
        task {
            printfn "getAppDomainInfo:1"
            let! (domainId, isDomainManagement, isTenantManagement) =
                query {
                    for app in dataContext.Applications do
                        where (app.ClientId = clientId)
                        select (app.DomainId, app.IsDomainManagement, app.Domain.Tenant <> null)
                }
                |> toSingleAsync

            printfn "getAppDomainInfo:2 %i %b %b" domainId isDomainManagement isTenantManagement

            let appDomainType =
                getAppDomainType isTenantManagement isDomainManagement

            return (domainId, appDomainType)
        }

    let getGenericAudienceScopes (dataContext: DbDataContext) userEmail domainId scopes =
        task {
            let! hasAssignedRole = userHasAssignedRole dataContext userEmail domainId

            match hasAssignedRole with
            // Given scopes returns these which assigned for particular user (intersection)
            | true -> return! getGenericDomainAudiencePermissions dataContext userEmail domainId scopes
            // Since user doesn't have any assigned role, return intersection of default and requested scopes
            | false -> return! getDefaultDomainAudiencePermissions dataContext domainId scopes
        }

    /// There is 2 type odf scopes in system
    /// 1. Management scopes shared between apis - used for management apis, every domain has single management api
    /// these scopes not expected be presented in request (TODO) and always fixes
    /// 2. Generic scopes - every scope belongs to particular api, function will return intersection of the requested scopes
    /// and scopes assigned for particular user, if no scope assigned to user, then all `default` scopes are considered assigned to him
    let private validateScopes' (dataContext: DbDataContext) userEmail clientId scopes =
        task {
            let! (domainId, appDomainType) = getAppDomainInfo dataContext clientId

            match appDomainType with
            // for generic app return intersection of requested scopes with assigned (or default)
            | Generic -> return! getGenericAudienceScopes dataContext userEmail domainId scopes
            // for managements app domain return default management scopes only
            // TODO : fix
            | _ ->
                // TODO : Management client does not request management scopes so don't check intersection
                let! validatedScopes = getManagementDomainPermissions dataContext userEmail domainId appDomainType
                let! audience = getManagementAudience dataContext domainId appDomainType

                return seq {
                           { Audience = audience
                             Scopes = validatedScopes }
                       }
        }

    let defaultAudienceScopes =
        { Audience = PERIMETER_USERS_AUDIENCE
          Scopes = [ "openid"; "profile"; "email" ] }

    let validateScopes (dataContext: DbDataContext) userEmail clientId scopes =
        task {
            if clientId = PERIMETER_CLIENT_ID then
                // For perimeter client we don't pay attention to requested scopes and always return predefined set
                // TODO : Fix
                return seq { defaultAudienceScopes }
            else
                let! audScopes = validateScopes' dataContext userEmail clientId scopes
                return Seq.append [ defaultAudienceScopes ] audScopes
        }
