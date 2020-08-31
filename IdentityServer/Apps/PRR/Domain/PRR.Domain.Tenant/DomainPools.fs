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
          DateCreated: System.DateTime
          Domains: DomainGetLike seq }

    let checkUserForbidden (userId: UserId) (domainPoolId: DomainPoolId) (dataContext: DbDataContext) =
        query {
            for dp in dataContext.DomainPools do
                where (dp.Id = domainPoolId && dp.Tenant.UserId = userId)
                select dp.Id
        }
        |> notFoundRaiseError Forbidden'

    //
    let guid () = Guid.NewGuid().ToString()

    type AuthConfig =
        { IdTokenExpiresIn: int<minutes>
          AccessTokenExpiresIn: int<minutes>
          RefreshTokenExpiresIn: int<minutes> }

    type Env =
        { DataContext: DbDataContext
          AuthStringsProvider: AuthStringsProvider
          AuthConfig: AuthConfig }

    let createDomainManagementApp (env: Env) domain =
        Application
            (Domain = domain,
             Name = "Domain Management Application",
             ClientId = env.AuthStringsProvider.ClientId(),
             ClientSecret = env.AuthStringsProvider.ClientSecret(),
             IdTokenExpiresIn = (int env.AuthConfig.IdTokenExpiresIn),
             RefreshTokenExpiresIn = (int env.AuthConfig.RefreshTokenExpiresIn),
             AllowedCallbackUrls = "*",
             Flow = FlowType.PKCE,
             IsDomainManagement = true)
        |> add' env.DataContext

    let createDomainManagementApi (env: Env) domain =
        Api
            (Domain = domain,
             Name = "Domain Management API",
             Identifier = sprintf "https://%s.management-api-%s.com" domain.EnvName (Guid.NewGuid().ToString()),
             IsDomainManagement = true,
             AccessTokenExpiresIn = (int env.AuthConfig.AccessTokenExpiresIn))
        |> add' env.DataContext

    let addUserRoles (userEmail: string) (domain: Domain) (roleIds: int seq) (dataContext: DbDataContext) =
        roleIds
        |> Seq.map (fun roleId -> DomainUserRole(UserEmail = userEmail, Domain = domain, RoleId = roleId))
        |> addRange dataContext

    let catch =
        function
        | UniqueConstraintException "IX_DomainPools_TenantId_Name" (ConflictErrorField ("name", UNIQUE)) ex -> raise ex
        | ex -> raise ex

    let create ((userId, tenantId, data): UserId * TenantId * PostLike) (env: Env) =
        let dataContext = env.DataContext

        let domainPool =
            DomainPool(TenantId = tenantId, Name = data.Name)
            |> add' dataContext

        let domain =
            Domain(Pool = domainPool, EnvName = "dev", IsMain = true)
            |> add' dataContext

        let _ = createDomainManagementApp env domain
        let _ = createDomainManagementApi env domain

        task {
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

                addUserRoles ownerEmail domain [ Seed.Roles.DomainOwner.Id ] dataContext
                addUserRoles userEmail domain [ Seed.Roles.DomainSuperAdmin.Id ] dataContext
            else
                addUserRoles ownerEmail domain [ Seed.Roles.DomainOwner.Id ] dataContext

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
