namespace PRR.Domain.Tenant

open Common.Domain.Models
open Common.Domain.Utils
open Common.Domain.Utils.CRUD
open Common.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open PRR.Data.Entities
open System
open System.Linq

module Domains =

    [<CLIMutable>]
    type PostLike =
        { EnvName: string }

    [<CLIMutable>]
    type ItemGetLike =
        { Id: int
          Name: string }

    [<CLIMutable>]
    type GetLike =
        { Id: int
          EnvName: string
          DateCreated: System.DateTime
          Applications: ItemGetLike seq
          Apis: ItemGetLike seq
          Roles: ItemGetLike seq }

    let checkUserForbidden (userId: UserId) (domainPoolId: DomainPoolId) (domainId: DomainId)
        (dataContext: DbDataContext) =
        query {
            for dp in dataContext.Domains do
                where (dp.Id = domainId && dp.PoolId = Nullable(domainPoolId) && dp.Pool.Tenant.UserId = userId)
                select dp.Id
        }
        |> notFoundRaiseError Forbidden'

    let catch =
        function
        | UniqueConstraintException "IX_Domains_PoolId_EnvName" (ConflictErrorField("envName", UNIQUE)) ex ->
            raise ex
        | ex ->
            raise ex

    type AuthConfig =
        { IdTokenExpiresIn: int<minutes>
          AccessTokenExpiresIn: int<minutes>
          RefreshTokenExpiresIn: int<minutes> }

    type Env =
        { AuthConfig: AuthConfig
          HashProvider: HashProvider }

    let private guid() = Guid.NewGuid().ToString()

    let private createDomainManagementApp (hashProvider: HashProvider) (domain, dataContext, authConfig: AuthConfig) =
        Application
            (Domain = domain, Name = "Domain Management Application", ClientId = guid(), ClientSecret = hashProvider(),
             IdTokenExpiresIn = (int authConfig.IdTokenExpiresIn),
             RefreshTokenExpiresIn = (int authConfig.RefreshTokenExpiresIn), AllowedCallbackUrls = "*",
             Flow = FlowType.PKCE, SSOEnabled = true, IsDomainManagement = true)

    let private createDomainManagementApi (domain, dataContext, authConfig: AuthConfig) =
        Api
            (Domain = domain, Name = "Domain Management API",
             Identifier = sprintf "https://%s.management-api-%s.com" domain.EnvName (Guid.NewGuid().ToString()),
             IsDomainManagement = true, AccessTokenExpiresIn = (int authConfig.AccessTokenExpiresIn))

    let private createUserDomainOwnerRole (userEmail: string) (domain: Domain) =
        DomainUserRole(UserEmail = userEmail, Domain = domain, RoleId = PRR.Data.DataContext.Seed.Roles.DomainOwner.Id)

    let create (env: Env): Create<DomainPoolId * PostLike * UserId, int, DbDataContext> =
        fun (domainPoolId, dto, userId) dataContext ->
            task {
                let domain =
                    Domain(PoolId = Nullable(domainPoolId), EnvName = dto.EnvName, IsMain = false) |> add' dataContext

                createDomainManagementApp env.HashProvider (domain, dataContext, env.AuthConfig) |> add dataContext

                createDomainManagementApi (domain, dataContext, env.AuthConfig) |> add dataContext

                let! userEmail = query {
                                     for user in dataContext.Users do
                                         where (user.Id = userId)
                                         select (user.Email)
                                 }
                                 |> toSingleExnAsync (unexpected "User email is not found")

                createUserDomainOwnerRole userEmail domain |> add dataContext

                try
                    do! saveChangesAsync dataContext
                    return domain.Id
                with ex ->
                    return catch ex
            }

    let update: Update<int, PostLike, DbDataContext> =
        updateCatch<Domain, _, _, _> catch (fun id -> Domain(Id = id))
            (fun dto entity -> entity.EnvName <- dto.EnvName)

    let remove: Remove<int, DbDataContext> =
        remove (fun id -> Role(Id = id))

    let getOne: GetOne<int, GetLike, DbDataContext> =
        getOne<Domain, _, _, _> (<@ fun p id -> p.Id = id @>)
            (<@ fun p ->
                { Id = p.Id
                  EnvName = p.EnvName
                  DateCreated = p.DateCreated
                  Applications =
                      p.Applications.Select(fun x ->
                          { Id = x.Id
                            Name = x.Name })
                  Apis =
                      p.Apis.Select(fun x ->
                          { Id = x.Id
                            Name = x.Name })
                  Roles =
                      p.Roles.Select(fun x ->
                          { Id = x.Id
                            Name = x.Name }) } @>)
