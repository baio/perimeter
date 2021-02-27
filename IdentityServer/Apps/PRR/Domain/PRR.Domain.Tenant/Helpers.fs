namespace PRR.Domain.Tenant

// open PRR.Domain.Models
open PRR.Data.Entities

[<AutoOpen>]
module Helpers =

    let createTenant name userId = Tenant(Name = name, UserId = userId)

    let createDomainPool tenant name identifier =
        DomainPool(Tenant = tenant, Name = name, Identifier = identifier)

    let createTenantManagementDomain (authStringProvider: IAuthStringsGetter) authConfig (tenant: Tenant) =
        Domain
            (Tenant = tenant,
             EnvName = "management",
             IsMain = true,
             Issuer = sprintf "https://management.%s.perimeter.pw/tenant/issuer" tenant.Name,
             AccessTokenExpiresIn = (int authConfig.AccessTokenExpiresIn),
             SigningAlgorithm = SigningAlgorithmType.HS256,
             HS256SigningSecret = authStringProvider.HS256SigningSecret())

    let createTenantManagementApp (authStringProvider: IAuthStringsGetter) (authConfig: AuthConfig) domain =
        Application
            (Domain = domain,
             Name = "Tenant domains management application",
             ClientId = authStringProvider.ClientId(),
             ClientSecret = authStringProvider.ClientSecret(),
             GrantTypes =
                 [| GrantType.AuthorizationCodePKCE
                    GrantType.RefreshToken
                    GrantType.AuthorizationCode
                    GrantType.ClientCredentials
                    GrantType.Password |],
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
             Identifier = sprintf "https://tenant-management-api.%s.%s.pw" domain.EnvName domain.Tenant.Name,
             IsDomainManagement = false)

    //
    let createMainDomain (authStringProviders: IAuthStringsGetter) authConfig (domainPool: DomainPool) =
        Domain
            (Pool = domainPool,
             EnvName = "dev",
             IsMain = true,
             AccessTokenExpiresIn = (int authConfig.AccessTokenExpiresIn),
             SigningAlgorithm = SigningAlgorithmType.RS256,
             HS256SigningSecret = authStringProviders.HS256SigningSecret(),
             RS256Params = authStringProviders.RS256XMLParams(),
             Issuer =
                 sprintf "https://dev.%s.%s.perimeter.pw/domain/issuer" domainPool.Identifier domainPool.Tenant.Name)

    let createDomainApp (authStringProvider: IAuthStringsGetter) (authConfig: AuthConfig) domain name =
        Application
            (Domain = domain,
             Name = name,
             ClientId = authStringProvider.ClientId(),
             ClientSecret = authStringProvider.ClientSecret(),
             IdTokenExpiresIn = (int authConfig.IdTokenExpiresIn),
             RefreshTokenExpiresIn = (int authConfig.RefreshTokenExpiresIn),
             AllowedCallbackUrls = "*",
             AllowedLogoutCallbackUrls = "*",
             GrantTypes =
                 [| GrantType.AuthorizationCodePKCE
                    GrantType.RefreshToken |],
             IsDomainManagement = false)

    let createDomainManagementApp (authStringProvider: IAuthStringsGetter) (authConfig: AuthConfig) domain =
        Application
            (Domain = domain,
             Name = "Domain management application",
             ClientId = authStringProvider.ClientId(),
             ClientSecret = authStringProvider.ClientSecret(),
             IdTokenExpiresIn = (int authConfig.IdTokenExpiresIn),
             RefreshTokenExpiresIn = (int authConfig.RefreshTokenExpiresIn),
             AllowedCallbackUrls = "*",
             AllowedLogoutCallbackUrls = "*",
             GrantTypes =
                 [| GrantType.AuthorizationCodePKCE
                    GrantType.RefreshToken
                    GrantType.AuthorizationCode
                    GrantType.ClientCredentials
                    GrantType.Password |],
             SSOEnabled = true,
             IsDomainManagement = true)

    let createDomainApi (authStringProvider: IAuthStringsGetter)
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
                     "https://%s.%s.%s.%s.perimeter.pw"
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
                     "https://domain-management-api.%s.%s.%s.perimeter.pw"
                     domain.EnvName
                     domain.Pool.Identifier
                     domain.Pool.Tenant.Name,
             IsDomainManagement = true)

    let createDomainUserRoles (userEmail: string) (domain: Domain) (roleIds: int seq) =
        roleIds
        |> Seq.map (fun roleId -> DomainUserRole(UserEmail = userEmail, Domain = domain, RoleId = roleId))

    let createDomainUserRole (userEmail: string) (domain: Domain) (roleId: int) =
        DomainUserRole(UserEmail = userEmail, Domain = domain, RoleId = roleId)
