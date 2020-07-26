namespace PRR.Domain.Tenant

open Common.Domain.Models
open Common.Domain.Models.Exceptions
open Common.Domain.Utils
open Common.Domain.Utils.CRUD
open Common.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open PRR.Data.Entities
open System
open System.Linq

module TenantUserRoles =

    [<CLIMutable>]
    type PostLike =
        { UserEmail: string
          RolesIds: int seq }

    [<CLIMutable>]
    type RolesGetLike =
        { Id: int
          Name: string }

    [<CLIMutable>]
    type GetLike =
        { Id: int
          Roles: RolesGetLike seq }

    let private validateRoles (rolesIds: int seq) (dataContext: DbDataContext) =
        query {
            for p in dataContext.Roles do
                where (p.IsTenantManagement <> true && (%in' (rolesIds)) p.Id)
                select p.Id
        }
        |> toCountAsync
        |> map (fun cnt ->
            if cnt > 0 then raise Forbidden)

    let private checkForbidenRoles (forbidenRolesRoles: int seq) (dto: PostLike) =
        dto.RolesIds
        |> Seq.filter (forbidenRolesRoles.Contains)
        |> Seq.length
        |> fun cnt ->
            if cnt > 0 then raise Forbidden

    let updateTenantRoles forbidenRoles ((tenantId, dto): TenantId * PostLike) (dbContext: DbDataContext) =
        task {

            checkForbidenRoles forbidenRoles dto

            // Find tenant's management domain
            let! domainId = query {
                                for domain in dbContext.Domains do
                                    where (domain.TenantId = Nullable(tenantId))
                                    select domain.Id
                            }
                            |> toSingleAsync

            do! validateRoles dto.RolesIds dbContext
            let incomingDur =
                dto.RolesIds
                |> Seq.map
                    (fun roleId -> DomainUserRole(RoleId = roleId, UserEmail = dto.UserEmail, DomainId = domainId))

            do! (query {
                     for dur in dbContext.DomainUserRole do
                         where (dur.UserEmail = dto.UserEmail && dur.DomainId = domainId)
                         select dur
                 }
                 |> querySplitAddRemoveRange dbContext incomingDur)

            do! saveChangesAsync dbContext
        }
