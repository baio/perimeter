﻿namespace PRR.Domain.Tenant

open Common.Domain.Models
open PRR.Data.Entities

[<AutoOpen>]
module Helpers =

    let createTenant name userId = Tenant(Name = name, UserId = userId)

    let createDomainPool tenant name identifier = DomainPool(Tenant = tenant, Name = name, Identifier = identifier)

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
             AllowedLogoutCallbackUrls = "*",
             IdTokenExpiresIn = (int authConfig.IdTokenExpiresIn),
             RefreshTokenExpiresIn = (int authConfig.RefreshTokenExpiresIn),
             SSOEnabled = true,
             IsDomainManagement = true)

    let createTenantManagementApi authConfig (domain: Domain)  =
        Api
            (Domain = domain,
             Name = "Tenant domains management API",
             Identifier = sprintf "https://tenant-management-api.%s.%s.com" domain.EnvName domain.Tenant.Name,
             IsDomainManagement = false,
             AccessTokenExpiresIn = (int authConfig.AccessTokenExpiresIn))

    //
    let createMainDomain (domainPool: DomainPool) =
        Domain(Pool = domainPool, EnvName = "dev", IsMain = true)

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
             ClientSecret = authStringProvider.ClientSecret(),
             IdTokenExpiresIn = (int authConfig.IdTokenExpiresIn),
             RefreshTokenExpiresIn = (int authConfig.RefreshTokenExpiresIn),
             AllowedCallbackUrls = "*",
             AllowedLogoutCallbackUrls = "*",
             Flow = FlowType.PKCE,
             IsDomainManagement = true)

    let createDomainApi (authConfig: AuthConfig) (domain: Domain) name identifier =
        Api
            (Domain = domain,
             Name = name,
             Identifier =
                 sprintf "https://%s.%s.%s.%s.com" identifier domain.EnvName domain.Pool.Identifier domain.Pool.Tenant.Name,
             IsDomainManagement = false,
             AccessTokenExpiresIn = (int authConfig.AccessTokenExpiresIn))

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
             IsDomainManagement = true,
             AccessTokenExpiresIn = (int authConfig.AccessTokenExpiresIn))

    let createDomainUserRoles (userEmail: string) (domain: Domain) (roleIds: int seq) =
        roleIds
        |> Seq.map (fun roleId -> DomainUserRole(UserEmail = userEmail, Domain = domain, RoleId = roleId))

    let createDomainUserRole (userEmail: string) (domain: Domain) (roleId: int) =
        DomainUserRole(UserEmail = userEmail, Domain = domain, RoleId = roleId)
