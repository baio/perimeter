﻿namespace PRR.Domain.Tenant

open PRR.Domain.Models
open DataAvail.Http.Exceptions
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open PRR.Data.Entities
open System
open System.ComponentModel.DataAnnotations
open System.Linq
open System.Threading.Tasks
open DataAvail.EntityFramework.Common
open DataAvail.ListQuery.Core
open DataAvail.ListQuery.EntityFramework
open DataAvail.Common.TaskUtils
open Z.EntityFramework.Plus

module DomainUserRoles =

    [<CLIMutable>]
    type PostLike =
        { [<EmailAddress>]
          [<Required>]
          UserEmail: string
          [<Required>]
          RolesIds: int seq }

    [<CLIMutable>]
    type RolesGetLike = { Id: int; Name: string }

    [<CLIMutable>]
    type GetLike =
        { UserEmail: string
          Roles: RolesGetLike seq }

    let private isUserDomainOwner (domainId: DomainId) (email: string) (dbContext: DbDataContext) =
        query {
            for dur in dbContext.DomainUserRole do
                where
                    (dur.DomainId = domainId
                     && dur.UserEmail = email
                     && dur.RoleId = Seed.Roles.DomainOwner.Id)

                select dur.UserEmail
        }
        |> countAsync
        |> map (function
            | 0 -> false
            | _ -> true)
    //if cnt = 1 then raise (forbidden "Domain owner could not be deleted or edited"))

    let private validateRoles (domainId: DomainId, rolesIds: int seq) (dataContext: DbDataContext) =
        query {
            for p in dataContext.Roles do
                where
                    ((p.Domain <> null
                      && p.DomainId <> Nullable(domainId))
                     && (%in' (rolesIds)) p.Id)

                select p.Id
        }
        |> toCountAsync
        |> map (fun cnt -> if cnt > 0 then raise Forbidden')

    let private checkForbiddenRoles (forbiddenRolesRoles: int seq) (dto: PostLike) =
        dto.RolesIds
        |> Seq.filter (forbiddenRolesRoles.Contains)
        |> Seq.length
        |> fun cnt -> if cnt > 0 then raise Forbidden'

    let updateRoles validateRolesFn forbiddenRoles ((domainId, dto): DomainId * PostLike) (dbContext: DbDataContext) =
        task {
            checkForbiddenRoles forbiddenRoles dto
            do! validateRolesFn (domainId, dto.RolesIds) dbContext

            let! isDomainOwner = isUserDomainOwner domainId dto.UserEmail dbContext

            // add fixed owner role if this is owner and owner role is missed from dto roles
            let fixedOwnerRoles =
                if isDomainOwner
                   && (dto.RolesIds.Contains Seed.Roles.DomainOwner.Id
                       |> not) then
                    [ Seed.Roles.DomainOwner.Id ]
                else
                    []

            let roleIds =
                dto.RolesIds |> Seq.append fixedOwnerRoles

            let incomingDur =
                roleIds
                |> Seq.map (fun roleId ->
                    DomainUserRole(RoleId = roleId, UserEmail = dto.UserEmail, DomainId = domainId))

            do! (query {
                     for dur in dbContext.DomainUserRole do
                         where
                             (dur.UserEmail = dto.UserEmail
                              && dur.DomainId = domainId)

                         select dur
                 }
                 |> querySplitAddRemoveRange dbContext incomingDur)

            do! saveChangesAsync dbContext
        }

    let updateUsersRoles x = x |> updateRoles validateRoles

    let getOne userEmail domainId (dataContext: DbDataContext) =
        query {
            for p in dataContext.DomainUserRole do
                where (p.DomainId = domainId && p.UserEmail = userEmail)
                select (Tuple.Create(p.UserEmail, { Id = p.Role.Id; Name = p.Role.Name }))
        }
        |> groupByAsync'
        |> map
            (Seq.tryHead
             >> function
             | Some (userEmail, roles) -> { UserEmail = userEmail; Roles = roles }
             | None -> raise NotFound)

    let remove (domainId: DomainId) (email: string) (dbContext: DbDataContext) =
        task {
            let q =
                query {
                    for dur in dbContext.DomainUserRole do
                        where
                            (dur.RoleId <> Seed.Roles.DomainOwner.Id
                             && dur.UserEmail = email
                             && dur.DomainId = domainId)

                        select dur
                }

            let! _ = q.DeleteAsync()

            return ()
        }

    //

    type SortField = UserEmail

    type FilterField = UserEmail

    type ListQuery = ListQuery<SortField, FilterField>

    [<CLIMutable>]
    type ListResponse = ListQueryResult<GetLike>

    type RoleType =
        | TenantManagement
        | DomainManagement
        | User

    type GetList = RoleType -> DbDataContext -> (DomainId * ListQuery) -> Task<ListResponse>

    [<CLIMutable>]
    type UserEmailData = { UserEmail: string }

    let getFilterFieldExpr filterValue =
        function
        | FilterField.UserEmail ->
            <@ fun (x: DomainUserRole) ->
                let like = %(ilike filterValue)
                like x.UserEmail @>

    let getSortFieldExpr =
        function
        | SortField.UserEmail -> SortString <@ fun (x: DomainUserRole) -> x.UserEmail @>

    let getList: GetList =
        fun roleType dataContext (domainId, prms) ->

            let dur =
                (query {
                    for p in dataContext.DomainUserRole do
                        select p
                 })

            let dur2 =
                handleListQuery dur getFilterFieldExpr getSortFieldExpr prms

            let dur3 =
                dur2.Select(fun x -> x.UserEmail).Distinct()

            let dur4 =
                dur.Select(fun x -> x.UserEmail).Distinct()

            let roleTypeFilter =
                match roleType with
                | TenantManagement -> <@ fun (x: Role) -> x.IsTenantManagement @>
                | DomainManagement -> <@ fun (x: Role) -> x.IsDomainManagement @>
                | User ->
                    <@ fun (x: Role) ->
                        not x.IsDomainManagement
                        && not x.IsTenantManagement @>

            query {
                for p0 in dur3 do
                    join p in dataContext.DomainUserRole on (p0 = p.UserEmail)
                    where (p.DomainId = domainId && (%roleTypeFilter) p.Role)
                    select p
            }
            |> handleSort' prms.Sort getSortFieldExpr
            |> fun q -> q.Select(fun p -> (Tuple.Create(p.UserEmail, { Id = p.Role.Id; Name = p.Role.Name })))
            |> executeGroupByQuery prms (fun (userEmail, roles) -> { UserEmail = userEmail; Roles = roles }) dur4
