namespace PRR.Domain.Tenant

// open PRR.Domain.Models
open PRR.Data.Entities

[<AutoOpen>]
module Helpers =


    let createTenant name userId = Tenant(Name = name, UserId = userId)

    let createDomainPool tenant name identifier =
        DomainPool(Tenant = tenant, Name = name, Identifier = identifier)

    let createTenantManagementDomain (authStringProvider: IAuthStringsGetter) authConfig (tenant: Tenant) =

        let envName = "dev"

        let issuerUri =
            authStringProvider.GetIssuerUri
                { TenantName = tenant.Name
                  DomainName = "tenant-management"
                  EnvName = envName }

        Domain
            (Tenant = tenant,
             EnvName = envName,
             IsMain = true,
             Issuer = issuerUri,
             AccessTokenExpiresIn = (int authConfig.AccessTokenExpiresIn),
             SigningAlgorithm = SigningAlgorithmType.HS256,
             HS256SigningSecret = authStringProvider.HS256SigningSecret())

    let createTenantManagementApp (authStringProvider: IAuthStringsGetter) (authConfig: AuthConfig) domain =
        Application
            (Domain = domain,
             Name = "tenant-management-app",
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

    let createTenantManagementApi (authStringProvider: IAuthStringsGetter) (domain: Domain) =
        let apiAudience =
            authStringProvider.GetAudienceUri
                { IssuerUriData =
                      { TenantName = domain.Tenant.Name
                        DomainName = "tenant-management"
                        EnvName = domain.EnvName }
                  ApiName = "tenant-management-api" }

        Api
            (Domain = domain,
             Name = "tenant-management-api",
             Identifier = apiAudience,
             IsDomainManagement = false)

    //
    let createMainDomain (authStringProviders: IAuthStringsGetter) authConfig (domainPool: DomainPool) =
        let envName = "dev"

        let issuerUri =
            authStringProviders.GetIssuerUri
                { TenantName = domainPool.Tenant.Name
                  DomainName = domainPool.Identifier
                  EnvName = envName }

        Domain
            (Pool = domainPool,
             EnvName = envName,
             IsMain = true,
             AccessTokenExpiresIn = (int authConfig.AccessTokenExpiresIn),
             SigningAlgorithm = SigningAlgorithmType.RS256,
             HS256SigningSecret = authStringProviders.HS256SigningSecret(),
             RS256Params = authStringProviders.RS256XMLParams(),
             Issuer = issuerUri)

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
             Name = "domain-management",
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

    let createDomainApi (authStringProvider: IAuthStringsGetter) (domain: Domain) name identifier =

        let apiAudience =
            authStringProvider.GetAudienceUri
                { IssuerUriData =
                      { TenantName = domain.Pool.Tenant.Name
                        DomainName = domain.Pool.Identifier
                        EnvName = domain.EnvName }
                  ApiName = identifier }

        Api(Domain = domain, Name = name, Identifier = apiAudience, IsDomainManagement = false)

    let createDomainManagementApi (authStringProvider: IAuthStringsGetter) (domain: Domain) =

        let apiAudience =
            authStringProvider.GetAudienceUri
                { IssuerUriData =
                      { TenantName = domain.Pool.Tenant.Name
                        DomainName = domain.Pool.Identifier
                        EnvName = domain.EnvName }
                  ApiName = "domain-management-api" }

        Api(Domain = domain, Name = "domain-management-api", Identifier = apiAudience, IsDomainManagement = true)

    let createDomainUserRoles (userEmail: string) (domain: Domain) (roleIds: int seq) =
        roleIds
        |> Seq.map (fun roleId -> DomainUserRole(UserEmail = userEmail, Domain = domain, RoleId = roleId))

    let createDomainUserRole (userEmail: string) (domain: Domain) (roleId: int) =
        DomainUserRole(UserEmail = userEmail, Domain = domain, RoleId = roleId)
