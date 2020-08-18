namespace PRR.Domain.Tenant

open Common.Domain.Models
open Common.Domain.Utils
open DomainUserRoles
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open PRR.Data.Entities
open System
open System.Threading.Tasks

module TenantUserRoles =

    let updateTenantRoles forbidenRoles ((tenantId, dto): TenantId * PostLike) (dbContext: DbDataContext) =
        task {
            let! domainId = query {
                                for domain in dbContext.Domains do
                                    where (domain.TenantId = Nullable(tenantId))
                                    select domain.Id
                            }
                            |> toSingleAsync

            return updateUsersRoles forbidenRoles (domainId, dto) dbContext
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
            return! remove domainId userEmail dataContext }
