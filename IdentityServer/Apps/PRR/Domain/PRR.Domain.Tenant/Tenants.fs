namespace PRR.Domain.Tenant

open DomainPools
open Common.Domain.Utils
open Common.Domain.Models
open Common.Domain.Utils.CRUD
open PRR.Data.DataContext
open PRR.Data.Entities
open FSharp.Control.Tasks.V2.ContextInsensitive

module Tenants =

    let catch =
        function
        | UniqueConstraintException "IX_Tenants_Name" (ConflictErrorField ("name", UNIQUE)) ex -> raise ex
        | ex -> raise ex

    type PostLike = { Name: string }

    let private createTenantManagementDomain (tenant: Tenant) =
        Domain(Tenant = tenant, EnvName = "management", IsMain = true)

    let private createTenantManagementApp (authStringProvider: AuthStringsProvider) authConfig domain =
        Application
            (Domain = domain,
             Name = "Tenant domains management app",
             ClientId = authStringProvider.ClientId(),
             ClientSecret = authStringProvider.ClientSecret(),
             Flow = FlowType.PKCE,
             AllowedCallbackUrls = "*",
             IdTokenExpiresIn = (int authConfig.IdTokenExpiresIn),
             RefreshTokenExpiresIn = (int authConfig.RefreshTokenExpiresIn),
             SSOEnabled = true,
             IsDomainManagement = true)

    let private createTenantManagementApi (domain: Domain) authConfig =
        Api
            (Domain = domain,
             Name = "Tenant domains management API",
             Identifier = sprintf "https://tenant-management-api.%s.%s.com" domain.EnvName domain.Tenant.Name,
             IsDomainManagement = false,
             AccessTokenExpiresIn = (int authConfig.AccessTokenExpiresIn))

    let create (env: Env) ((userId, data): UserId * PostLike) =
        task {
            let dataContext = env.DataContext

            let add x = x |> add dataContext
            let add' x = x |> add' dataContext

            let tenant =
                Tenant(Name = data.Name, UserId = userId) |> add'

            let domainPool =
                DomainPool(Tenant = tenant, Name = data.Name)
                |> add'

            // tenant management
            let tenantManagementDomain =
                createTenantManagementDomain tenant |> add'

            createTenantManagementApp env.AuthStringsProvider env.AuthConfig tenantManagementDomain
            |> add

            createTenantManagementApi tenantManagementDomain env.AuthConfig
            |> add

            // domain management
            let domain =
                Domain(Pool = domainPool, EnvName = "dev", IsMain = true)
                |> add'

            let _ = createDomainManagementApp env domain
            let _ = createDomainManagementApi env domain

            try
                do! saveChangesAsync dataContext
                return tenant.Id
            with ex -> return catch ex
        }
