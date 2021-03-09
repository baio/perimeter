namespace PRR.Domain.Auth.Common

open System
open DataAvail.Common
open Microsoft.Extensions.Logging
open PRR.Data.DataContext
open PRR.Domain.Models
open FSharp.Control.Tasks.V2.ContextInsensitive
open DataAvail.EntityFramework.Common
open DataAvail.Http.Exceptions

[<AutoOpen>]
module ValidateScopes =

    type ValidateScopesEnv =
        { Logger: ILogger
          DataContext: DbDataContext }

    type private AppDomainType =
        | TenantManagement
        | DomainManagement
        | Generic

    let private getTenantManagementAudience { DataContext = dataContext } domainId =
        query {
            for api in dataContext.Apis do
                where (api.DomainId = domainId)
                select api.Identifier
        }
        |> toSingleAsync

    let private getDomainManagementAudience { DataContext = dataContext } domainId =
        query {
            for api in dataContext.Apis do
                where
                    (api.DomainId = domainId
                     && api.IsDomainManagement = true)

                select api.Identifier
        }
        |> toSingleAsync

    /// For domain and tenant management apis returns audience and shared permissions (scopes)
    let private getManagementAudience env domainId =
        function
        | TenantManagement -> getTenantManagementAudience env domainId
        | DomainManagement -> getDomainManagementAudience env domainId

    let private userHasCommonAssignedRole { DataContext = dataContext } userEmail domainId =
        query {
            for dur in dataContext.DomainUserRole do
                where
                    (dur.UserEmail = userEmail
                     && dur.DomainId = domainId
                     && not dur.Role.IsDomainManagement
                     && not dur.Role.IsTenantManagement)
                select dur.RoleId
        }
        |> toCountAsync
        |> TaskUtils.map (fun i -> i > 0)

    let private getDefaultDomainAudiencePermissions { DataContext = dataContext } domainId scopes =
        query {
            for p in dataContext.Permissions do
                where
                    (p.IsDefault = true
                     && p.Api.DomainId = domainId
                     && (%in' scopes) p.Name)

                select (Tuple.Create(p.Api.Identifier, p.Name))
        }
        |> groupByAsync (fun (aud, perms) -> { Audience = aud; Scopes = perms })

    let private getGenericDomainAudiencePermissions { DataContext = dataContext } userEmail domainId scopes =
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

    let private getManagementDomainPermissions { DataContext = dataContext } userEmail domainId appDomainType =
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

    let private getAppDomainInfo { DataContext = dataContext } clientId =
        task {
            let! result =
                query {
                    for app in dataContext.Applications do
                        where (app.ClientId = clientId)
                        select (app.DomainId, app.IsDomainManagement, app.Domain.Tenant <> null)
                }
                |> toSingleOptionAsync

            match result with
            | Some (domainId, isDomainManagement, isTenantManagement) ->
                let appDomainType =
                    getAppDomainType isTenantManagement isDomainManagement

                return Some(domainId, appDomainType)
            | None -> return None
        }

    let getGenericAudienceScopes env userEmail domainId scopes =
        task {
            env.Logger.LogDebug("GetGenericAudienceScopes {userEmail} {domainId} {scopes}", domainId, domainId, scopes)

            let! hasAssignedRole = userHasCommonAssignedRole env userEmail domainId

            match hasAssignedRole with
            // Given scopes returns these which assigned for particular user (intersection)
            | true ->
                env.Logger.LogDebug("Given scopes returns these which assigned for particular user (intersection)")
                return! getGenericDomainAudiencePermissions env userEmail domainId scopes
            // Since user doesn't have any assigned role, return intersection of default and requested scopes
            | false ->
                env.Logger.LogDebug
                    ("Since user doesn't have any assigned role, return intersection of default and requested scopes")

                return! getDefaultDomainAudiencePermissions env domainId scopes
        }

    /// There is 2 type odf scopes in system
    /// 1. Management scopes shared between apis - used for management apis, every domain has single management api
    /// these scopes not expected be presented in request (TODO) and always fixes
    /// 2. Generic scopes - every scope belongs to particular api, function will return intersection of the requested scopes
    /// and scopes assigned for particular user, if no scope assigned to user, then all `default` scopes are considered assigned to him
    let private validateScopes' env userEmail clientId scopes =
        task {
            let! result = getAppDomainInfo env clientId

            match result with
            | Some (domainId, appDomainType) ->
                env.Logger.LogDebug("App info {domainId} {appDomainType}", domainId, appDomainType)

                match appDomainType with
                // for generic app return intersection of requested scopes with assigned (or default)
                | Generic -> return! getGenericAudienceScopes env userEmail domainId scopes
                // for managements app domain return default management scopes plus requested ones
                // TODO : fix
                | _ ->
                    // TODO : Management client does not request management scopes so don't check intersection
                    let! validatedScopes = getManagementDomainPermissions env userEmail domainId appDomainType
                    let! audience = getManagementAudience env domainId appDomainType

                    return
                        (seq {
                            { Audience = audience
                              Scopes = validatedScopes }
                         })
            | None ->
                env.Logger.LogDebug("App info not found")
                return raise (unAuthorized (sprintf "App info is not found for client_id %s" clientId))
        }

    let private openIdScopes =
        [ "openid"
          "profile"
          "email"
          "offline_access" ]

    let private getOpenIdIntersections scopes =
        scopes
        |> Seq.filter (fun x -> Seq.contains x openIdScopes)

    let private appendOpenIDScopes requestedScopes foundScopes =

        getOpenIdIntersections requestedScopes
        |> Seq.append foundScopes

    // Default audience to manage tenant
    let defaultAudienceScopes =
        { Audience = PERIMETER_USERS_AUDIENCE
          Scopes =
              [ "openid"
                "profile"
                "email"
                "offline_access" ] }

    let validateScopes (env: ValidateScopesEnv) userEmail clientId scopes =
        env.Logger.LogDebug("Validate scopes with {userEmail} {clientId} {@scopes}", userEmail, clientId, scopes)

        task {
            if clientId = PERIMETER_CLIENT_ID then
                // For perimeter client we don't pay attention to requested scopes and always return predefined set
                // TODO : Fix
                env.Logger.LogDebug("This is perimeter client")

                return
                    seq {
                        { Audience = PERIMETER_USERS_AUDIENCE
                          Scopes = openIdScopes }
                    }
            else
                env.Logger.LogDebug("This is generic client")

                let! audScopes = validateScopes' env userEmail clientId scopes

                env.Logger.LogDebug("Regular scopes result {@result}", audScopes)

                let result =
                    audScopes
                    |> Seq.map (fun audScope ->
                        { audScope with
                              Scopes = audScope.Scopes |> appendOpenIDScopes scopes })

                env.Logger.LogDebug("Joint scopes result {@result}", result)

                return result |> Seq.append [ defaultAudienceScopes ]
        }
