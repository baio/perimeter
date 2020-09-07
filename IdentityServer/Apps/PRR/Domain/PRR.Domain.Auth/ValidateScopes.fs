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

    let getDefaultScopes (dataContext: DbDataContext) domainId isTenantManagement isDomainManagement =
        task {
            if isTenantManagement
            then return! getTenantManagementAudiencePermissions dataContext domainId
            else if isDomainManagement
            then return! getDomainManagementAudiencePermissions dataContext domainId
            else return Seq.empty
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

    let private getScopeAudience (scopeAudiences: AudienceScopes seq) scope =
        let res =
            scopeAudiences
            |> Seq.tryFind (fun x -> x.Scopes |> Seq.contains scope)

        match res with
        | Some x -> x.Audience
        | None -> raise (unexpected "Audience not found by scope")

    let private validateScopes'' (dataContext: DbDataContext) userEmail clientId scopes =
        task {

            let! (domainId, isDomainManagement, isTenantManagement) =
                query {
                    for app in dataContext.Applications do
                        where (app.ClientId = clientId)
                        select (app.DomainId, app.IsDomainManagement, app.Domain.Tenant <> null)
                }
                |> toSingleAsync

            let! defaultAudienceScopes = getDefaultScopes dataContext domainId isTenantManagement isDomainManagement

            let defaultScopes =
                defaultAudienceScopes
                |> Seq.collect (fun x -> x.Scopes)

            let requestedScopes = Seq.append scopes defaultScopes

            let! audScopes = getDomainAudiencePermissions dataContext userEmail domainId requestedScopes

            let audScopesWithAudiences =
                audScopes
                |> Seq.filter (fun x -> x.Audience <> null)
            // Management scopes don't have linked audiences
            // Restore managements scope audiences manually
            let audScopesNoAudiences =
                audScopes
                |> Seq.filter (fun x -> x.Audience = null)
                |> Seq.collect (fun x -> x.Scopes)
                |> Seq.map (fun x -> (getScopeAudience defaultAudienceScopes x), x)
                |> Seq.groupBy (fst)
                |> Seq.map (fun (x, y) ->
                    { Audience = x
                      Scopes = y |> Seq.map snd })

            return Seq.append audScopesWithAudiences audScopesNoAudiences
        }

    let validateScopes' (dataContext: DbDataContext) userEmail clientId scopes =
        task {
            if clientId = PERIMETER_CLIENT_ID then
                return seq {
                           { Audience = PERIMETER_USERS_AUDIENCE
                             Scopes = [] }
                       }
            else
                return! validateScopes'' dataContext userEmail clientId scopes
        }

    let defaultAudienceScopes =
        { Audience = PERIMETER_USERS_AUDIENCE
          Scopes = [ "openid"; "profile" ] }

    let validateScopes (dataContext: DbDataContext) userEmail clientId scopes =
        task {
            if clientId = PERIMETER_CLIENT_ID then
                return seq {
                           { Audience = PERIMETER_USERS_AUDIENCE
                             Scopes = [ "openid"; "profile" ] }
                       }
            else
                let! audScopes = validateScopes' dataContext userEmail clientId scopes
                return Seq.append [ defaultAudienceScopes ] audScopes
        }
