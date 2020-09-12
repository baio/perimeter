namespace PRR.Domain.Tenant

open Common.Domain.Models
open Common.Domain.Utils
open Common.Utils.TaskUtils
open DomainUserRoles
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open System
open System.Threading.Tasks

module TenantUserRoles =

    let private checkUserNotTenantOwner (domainId: DomainId) (email: string) (dbContext: DbDataContext) =
        query {
            for dur in dbContext.DomainUserRole do
                where (dur.DomainId = domainId && dur.UserEmail = email && dur.RoleId = Seed.Roles.TenantOwner.Id)
                select dur.UserEmail
        }
        |> countAsync
        |> map (fun cnt ->
            if cnt = 1 then raise (forbidden "Tenant owner could not be deleted or edited"))

    let private validateRoles (_, rolesIds: int seq) (dataContext: DbDataContext) =
        query {
            for p in dataContext.Roles do
                where (p.IsTenantManagement <> true && (%in' (rolesIds)) p.Id)
                select p.Id
        }
        |> toCountAsync
        |> map (fun cnt ->
            if cnt > 0 then raise Forbidden')

    let updateTenantRoles forbiddenRoles ((tenantId, dto): TenantId * PostLike) (dbContext: DbDataContext) =
        task {
            let! domainId = query {
                                for domain in dbContext.Domains do
                                    where (domain.TenantId = Nullable(tenantId))
                                    select domain.Id
                            }
                            |> toSingleAsync

            do! checkUserNotTenantOwner domainId dto.UserEmail dbContext
            return updateRoles validateRoles forbiddenRoles (domainId, dto) dbContext
        }

    let private getUsersTenantManagementDomain userId (dbContext: DbDataContext) =
        query {
            for domain in dbContext.Domains do
                where (domain.Tenant.UserId = userId)
                select domain.Id
        }
        |> toSingleAsync

    type GetList = DbDataContext -> (UserId * ListQuery) -> Task<ListResponse>

    let getList: GetList =
        fun dataContext (userId, prms) ->
            task {
                let! domainId = getUsersTenantManagementDomain userId dataContext
                return! getList RoleType.TenantManagement dataContext (domainId, prms) }

    let getOne userEmail userId (dataContext: DbDataContext) =
        task {
            let! domainId = getUsersTenantManagementDomain userId dataContext
            return! getOne userEmail domainId dataContext }

    let remove userEmail userId (dataContext: DbDataContext) =
        task {
            let! domainId = getUsersTenantManagementDomain userId dataContext
            do! checkUserNotTenantOwner domainId userEmail dataContext
            return! remove domainId userEmail dataContext
        }
