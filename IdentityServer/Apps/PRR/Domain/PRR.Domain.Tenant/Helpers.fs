namespace PRR.Domain.Tenant

open Common.Domain.Models
open PRR.Data.Entities

[<AutoOpen>]
module internal Helpers =

    let createTenant name userId = Tenant(Name = name, UserId = userId)

    let createDomainPool tenant name = DomainPool(Tenant = tenant, Name = name)

    let createDomainPool' tenantId name = DomainPool(TenantId = tenantId)

    let createTenantManagementDomain (tenant: Tenant) =
        Domain(Tenant = tenant, EnvName = "management", IsMain = true)

    let createTenantManagementApp (authStringProvider: AuthStringsProvider) (authConfig: AuthConfig) domain =
        Application
            (Domain = domain,
             Name = "Tenant domains management application",
             ClientId = authStringProvider.ClientId(),
             ClientSecret = authStringProvider.ClientSecret(),
             Flow = FlowType.PKCE,
             AllowedCallbackUrls = "*",
             IdTokenExpiresIn = (int authConfig.IdTokenExpiresIn),
             RefreshTokenExpiresIn = (int authConfig.RefreshTokenExpiresIn),
             SSOEnabled = true,
             IsDomainManagement = true)

    let createTenantManagementApi (domain: Domain) authConfig =
        Api
            (Domain = domain,
             Name = "Tenant domains management API",
             Identifier = sprintf "https://tenant-management-api.%s.%s.com" domain.EnvName domain.Tenant.Name,
             IsDomainManagement = false,
             AccessTokenExpiresIn = (int authConfig.AccessTokenExpiresIn))

    //
    let createMainDomain (domainPool: DomainPool) =
        Domain(Pool = domainPool, EnvName = "dev", IsMain = true)
    
    let createDomainManagementApp (authStringProvider: AuthStringsProvider) (authConfig: AuthConfig) domain =
        Application
            (Domain = domain,
             Name = "Domain management application",
             ClientId = authStringProvider.ClientId(),
             ClientSecret = authStringProvider.ClientSecret(),
             IdTokenExpiresIn = (int authConfig.IdTokenExpiresIn),
             RefreshTokenExpiresIn = (int authConfig.RefreshTokenExpiresIn),
             AllowedCallbackUrls = "*",
             Flow = FlowType.PKCE,
             IsDomainManagement = true)

    let createDomainManagementApi (authConfig: AuthConfig) (domain: Domain) =
        Api
            (Domain = domain,
             Name = "Domain management API",
             Identifier = sprintf "https://domain-management-api.%s.%scom" domain.EnvName domain.Pool.Tenant.Name,
             IsDomainManagement = true,
             AccessTokenExpiresIn = (int authConfig.AccessTokenExpiresIn))

    let createDomainUserRoles (userEmail: string) (domain: Domain) (roleIds: int seq) =
        roleIds
        |> Seq.map (fun roleId -> DomainUserRole(UserEmail = userEmail, Domain = domain, RoleId = roleId))

    let createDomainUserRole (userEmail: string) (domain: Domain) (roleId: int) =
        DomainUserRole(UserEmail = userEmail, Domain = domain, RoleId = roleId)
