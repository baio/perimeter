namespace PRR.Domain.Tenant

open Common.Domain.Models
open Common.Domain.Utils
open Common.Domain.Utils.CRUD
open Common.Domain.Utils.UniqueConstraintException
open Common.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FSharpx.Linq
open Microsoft.EntityFrameworkCore
open PRR.Data.DataContext
open PRR.Data.Entities
open System
open System.Linq
open System.Linq.Expressions
open System.Threading.Tasks
open Helpers

module DomainPools =

    [<CLIMutable>]
    type PostLike = { Name: string }

    [<CLIMutable>]
    type DomainGetLike =
        { Id: int
          IsMain: Boolean
          EnvName: string
          DomainManagementClientId: ClientId }

    [<CLIMutable>]
    type GetLike =
        { Id: int
          Name: string
          DateCreated: DateTime
          Domains: DomainGetLike seq }

    let checkUserForbidden (userId: UserId) (domainPoolId: DomainPoolId) (dataContext: DbDataContext) =
        query {
            for dp in dataContext.DomainPools do
                where (dp.Id = domainPoolId && dp.Tenant.UserId = userId)
                select dp.Id
        }
        |> notFoundRaiseError Forbidden'

    //

    let catch =
        function
        | UniqueConstraintException "IX_DomainPools_TenantId_Name" (ConflictErrorField ("name", UNIQUE)) ex -> raise ex
        | ex -> raise ex
        
    let validateData (data: PostLike) =
        [| (validateDomainName "name" data.Name) |] 
        |> Array.choose id
        
    let create ((userId, tenantId, data): UserId * TenantId * PostLike) (env: Env) =
        let dataContext = env.DataContext

        let add x = x |> add dataContext

        let add' x = x |> add' dataContext

        task {

            let! tenant =
                query {
                    for tenant in dataContext.Tenants do
                        where (tenant.Id = tenantId)
                        select (Tenant(Id = tenant.Id, Name = tenant.Name))
                }
                |> toSingleUnchangedAsync dataContext

            let domainPool =
                createDomainPool tenant data.Name |> add'

            let domain = createMainDomain domainPool |> add'

            createDomainManagementApp env.AuthStringsProvider env.AuthConfig domain
            |> add

            createDomainManagementApi env.AuthConfig domain
            |> add

            let! (ownerId, ownerEmail) =
                query {
                    for tenant in dataContext.Tenants do
                        where (tenant.Id = tenantId)
                        select (System.Tuple<_, _>(tenant.UserId, tenant.User.Email))
                }
                |> toSingleAsync

            // Tenant owner -> Domain owner
            // User -> Domain super admin
            if ownerId <> userId then
                let! userEmail =
                    query {
                        for user in dataContext.Users do
                            where (user.Id = userId)
                            select user.Email
                    }
                    |> toSingleAsync

                createDomainUserRole ownerEmail domain Seed.Roles.DomainOwner.Id
                |> add

                createDomainUserRole userEmail domain Seed.Roles.DomainSuperAdmin.Id
                |> add
            else
                createDomainUserRole ownerEmail domain Seed.Roles.DomainOwner.Id
                |> add

            try
                do! saveChangesAsync dataContext
            with ex -> return catch ex

            return domainPool.Id
        }

    //
    let update: Update<int, PostLike, DbDataContext> =
        updateCatch<DomainPool, _, _, _> catch (fun id -> DomainPool(Id = id)) (fun dto entity ->
            entity.Name <- dto.Name)

    let remove: Remove<int, DbDataContext> = remove (fun id -> DomainPool(Id = id))

    let private selectGet =
        <@ fun (p: DomainPool) ->
            { Id = p.Id
              Name = p.Name
              DateCreated = p.DateCreated
              Domains =
                  p.Domains.Select(fun x ->
                      { Id = x.Id
                        IsMain = x.IsMain
                        EnvName = x.EnvName
                        DomainManagementClientId = x.Applications.First(fun f -> f.IsDomainManagement).ClientId }) } @>


    let getOne: GetOne<int, GetLike, DbDataContext> =
        getOne<DomainPool, _, _, _> (<@ fun p id -> p.Id = id @>) selectGet

    //
    type SortField =
        | Name
        | DateCreated

    type FilterField = Name

    type ListQuery = ListQuery<SortField, FilterField>

    [<CLIMutable>]
    type ListResponse = ListResponse<GetLike>

    type GetList = DbDataContext -> (TenantId * ListQuery) -> Task<ListResponse>

    let getFilterFieldExpr filterValue =
        function
        | FilterField.Name ->
            <@ fun (domain: DomainPool) ->
                let like = %(ilike filterValue)
                like domain.Name @>

    let getSortFieldExpr =
        function
        | SortField.DateCreated -> SortDate <@ fun (domain: DomainPool) -> domain.DateCreated @>
        | SortField.Name -> SortString <@ fun (domain: DomainPool) -> domain.Name @>

    let getList: GetList =
        fun dataContext (tenantId, prms) ->

            let domainPools =
                handleListQuery dataContext.DomainPools getFilterFieldExpr getSortFieldExpr prms

            query {
                for domainPool in domainPools do
                    where (domainPool.TenantId = tenantId)
                    select ((%selectGet) domainPool)
            }
            |> executeListQuery prms
