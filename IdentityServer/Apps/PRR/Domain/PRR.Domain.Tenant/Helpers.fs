namespace PRR.Domain.Tenant

open Common.Domain.Models
open PRR.Data.Entities

[<AutoOpen>]
module Helpers =

    let createTenant name userId = Tenant(Name = name, UserId = userId)

    let createDomainPool tenant name identifier =
        DomainPool(Tenant = tenant, Name = name, Identifier = identifier)

    let createTenantManagementDomain authConfig (tenant: Tenant) =
        Domain
            (Tenant = tenant,
             EnvName = "management",
             IsMain = true,
             Issuer = sprintf "https://management.%s.perimeter.com/tenant/issuer" tenant.Name,
             AccessTokenExpiresIn = (int authConfig.AccessTokenExpiresIn),
             SigningAlgorithm = SigningAlgorithmType.HS256,
             HS256SigningSecret = authConfig.AccessTokenSecret)

    let createTenantManagementApp (authStringProvider: AuthStringsProvider) (authConfig: AuthConfig) domain =
        Application
            (Domain = domain,
             Name = "Tenant domains management application",
             ClientId = authStringProvider.ClientId(),
             ClientSecret = authStringProvider.ClientSecret(),
             Flow = FlowType.PKCE,
             AllowedCallbackUrls = "*",
             AllowedLogoutCallbackUrls = "*",
             IdTokenExpiresIn = (int authConfig.IdTokenExpiresIn),
             RefreshTokenExpiresIn = (int authConfig.RefreshTokenExpiresIn),
             SSOEnabled = true,
             IsDomainManagement = true)

    let createTenantManagementApi authConfig (domain: Domain) =
        Api
            (Domain = domain,
             Name = "Tenant domains management API",
             Identifier = sprintf "https://tenant-management-api.%s.%s.com" domain.EnvName domain.Tenant.Name,
             IsDomainManagement = false)

    //
    let createMainDomain authConfig (domainPool: DomainPool) =
        Domain
            (Pool = domainPool,
             EnvName = "dev",
             IsMain = true,
             AccessTokenExpiresIn = (int authConfig.AccessTokenExpiresIn),
             SigningAlgorithm = SigningAlgorithmType.HS256,
             HS256SigningSecret = authConfig.AccessTokenSecret,
             Issuer =
                 sprintf "https://dev.%s.%s.perimeter.com/domain/issuer" domainPool.Identifier domainPool.Tenant.Name)

    let createDomainApp (authStringProvider: AuthStringsProvider) (authConfig: AuthConfig) domain name =
        Application
            (Domain = domain,
             Name = name,
             ClientId = authStringProvider.ClientId(),
             ClientSecret = authStringProvider.ClientSecret(),
             IdTokenExpiresIn = (int authConfig.IdTokenExpiresIn),
             RefreshTokenExpiresIn = (int authConfig.RefreshTokenExpiresIn),
             AllowedCallbackUrls = "*",
             AllowedLogoutCallbackUrls = "*",
             Flow = FlowType.PKCE,
             IsDomainManagement = false)

    let createDomainManagementApp (authStringProvider: AuthStringsProvider) (authConfig: AuthConfig) domain =
        Application
            (Domain = domain,
             Name = "Domain management application",
             ClientId = authStringProvider.ClientId(),
             ClientSecret = authConfig.AccessTokenSecret,
             IdTokenExpiresIn = (int authConfig.IdTokenExpiresIn),
             RefreshTokenExpiresIn = (int authConfig.RefreshTokenExpiresIn),
             AllowedCallbackUrls = "*",
             AllowedLogoutCallbackUrls = "*",
             Flow = FlowType.PKCE,
             SSOEnabled = true,
             IsDomainManagement = true)

    let createDomainApi (authStringProvider: AuthStringsProvider)
                        (authConfig: AuthConfig)
                        (domain: Domain)
                        name
                        identifier
                        =
        Api
            (Domain = domain,
             Name = name,
             Identifier =
                 sprintf
                     "https://%s.%s.%s.%s.com"
                     identifier
                     domain.EnvName
                     domain.Pool.Identifier
                     domain.Pool.Tenant.Name,
             IsDomainManagement = false)

    let createDomainManagementApi (authConfig: AuthConfig) (domain: Domain) =
        Api
            (Domain = domain,
             Name = "Domain management API",
             Identifier =
                 sprintf
                     "https://domain-management-api.%s.%s.%s.com"
                     domain.EnvName
                     domain.Pool.Identifier
                     domain.Pool.Tenant.Name,
             IsDomainManagement = true)

    let createDomainUserRoles (userEmail: string) (domain: Domain) (roleIds: int seq) =
        roleIds
        |> Seq.map (fun roleId -> DomainUserRole(UserEmail = userEmail, Domain = domain, RoleId = roleId))

    let createDomainUserRole (userEmail: string) (domain: Domain) (roleId: int) =
        DomainUserRole(UserEmail = userEmail, Domain = domain, RoleId = roleId)
