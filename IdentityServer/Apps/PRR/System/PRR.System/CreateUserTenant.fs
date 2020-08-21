namespace PRR.System

open Akkling

open Common.Domain.Models
open Common.Domain.Utils.LinqHelpers
open Common.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FSharpx.Reader
open PRR.Data.DataContext
open PRR.Data.Entities
open PRR.System.Models
open System
open System.Threading.Tasks

[<AutoOpen>]
module private CreateUserTenant =

    type Env =
        { GetDataContextProvider: GetDataContextProvider
          HashProvider: HashProvider
          AuthConfig: AuthConfig }

    let guid() = Guid.NewGuid().ToString()

    let createTenant (data: SignUpConfirmSuccess) (dataContext: DbDataContext) =
        Tenant(UserId = data.UserId, Name = data.Email) |> add' dataContext

    let createDomainPool tenant (dataContext: DbDataContext) =
        DomainPool(Tenant = tenant, Name = "New Domain") |> add' dataContext

    let createDomain (domainPool: DomainPool) (dataContext: DbDataContext) =
        Domain(Pool = domainPool, EnvName = "test", IsMain = true) |> add' dataContext

    let createApp (hashProvider: HashProvider) (domain, dataContext, authConfig) =
        Application
            (Domain = domain, Name = "New Application", ClientId = guid(), ClientSecret = hashProvider(),
             IdTokenExpiresIn = (int authConfig.IdTokenExpiresIn),
             RefreshTokenExpiresIn = (int authConfig.RefreshTokenExpiresIn), AllowedCallbackUrls = "*",
             Flow = FlowType.PKCE) |> add' dataContext

    let createApi (domain, dataContext, authConfig) =
        Api
            (Domain = domain, Name = "New API",
             Identifier = sprintf "https://%s.new-domain-%s.com" domain.EnvName (Guid.NewGuid().ToString()),
             AccessTokenExpiresIn = (int authConfig.AccessTokenExpiresIn)) |> add' dataContext

    let createDomainManagementApp (hashProvider: HashProvider) (domain, dataContext, authConfig) =
        Application
            (Domain = domain, Name = "Domain Management Application", ClientId = guid(), ClientSecret = hashProvider(),
             IdTokenExpiresIn = (int authConfig.IdTokenExpiresIn),
             RefreshTokenExpiresIn = (int authConfig.RefreshTokenExpiresIn), AllowedCallbackUrls = "*",
             Flow = FlowType.PKCE, SSOEnabled = true) |> add' dataContext

    let createDomainManagementApi (domain, dataContext, authConfig) =
        Api
            (Domain = domain, Name = "Domain Management API",
             Identifier = sprintf "https://%s.management-api-%s.com" domain.EnvName (Guid.NewGuid().ToString()),
             IsUserManagement = true, AccessTokenExpiresIn = (int authConfig.AccessTokenExpiresIn)) |> add' dataContext

    let addUserRoles (userEmail: string) (domain: Domain) (roleIds: int seq) (dataContext: DbDataContext) =
        roleIds
        |> Seq.map (fun roleId -> DomainUserRole(UserEmail = userEmail, Domain = domain, RoleId = roleId))
        |> addRange dataContext

    //
    let createTenantManagementDomain (tenant: Tenant) (dataContext: DbDataContext) =
        Domain(Tenant = tenant, EnvName = "management", IsMain = true) |> add' dataContext

    let createTenantManagementApp (hashProvider: HashProvider) (domain, dataContext, authConfig) =
        Application
            (Domain = domain, Name = "Tenant domains management app", ClientId = guid(), ClientSecret = hashProvider(),
             Flow = FlowType.PKCE, AllowedCallbackUrls = "*", IdTokenExpiresIn = (int authConfig.IdTokenExpiresIn),
             RefreshTokenExpiresIn = (int authConfig.RefreshTokenExpiresIn), SSOEnabled = true) |> add' dataContext

    let createTenantManagementApi (domain, dataContext, authConfig) =
        Api
            (Domain = domain, Name = "Tenant domains management API",
             Identifier = sprintf "https://tenant-management-api-%s.com" (Guid.NewGuid().ToString()),
             IsUserManagement = false, AccessTokenExpiresIn = (int authConfig.AccessTokenExpiresIn))
        |> add' dataContext


    /// Creates tenant, domain pool, domain, new app, new api and new users management api
    /// tenant -
    /// domain pool -
    /// app -
    /// api -
    /// users management api -
    let createUserTenant (env: Env) data =

        let hashProvider = env.HashProvider
        let authConfig = env.AuthConfig

        task {

            use dpr = env.GetDataContextProvider()
            let dataContext = dpr.DataContext

            let tenant = createTenant data dataContext

            //
            let tenantManagementDomain = createTenantManagementDomain tenant dataContext

            let (tenantManagementApp, tenantManagementApi) =
                (doublet <!> (createTenantManagementApp hashProvider) <*> createTenantManagementApi)
                <| (tenantManagementDomain, dataContext, authConfig)

            let tenantManagementRoles = [ Seed.Roles.TenantOwner.Id ]
            addUserRoles data.Email tenantManagementDomain tenantManagementRoles dataContext

            //
            let sampleDomain =
                (createDomainPool tenant >>= createDomain) <| dataContext

            let (usersManagementApp, usersManagementApi) =
                (doublet <!> (createDomainManagementApp hashProvider) <*> createDomainManagementApi)
                <| (sampleDomain, dataContext, authConfig)

            let domainManagementRoles = [ Seed.Roles.DomainOwner.Id ]
            addUserRoles data.Email sampleDomain domainManagementRoles dataContext

            //
            let (sampeApp, sampleApi) =
                (doublet <!> (createApp hashProvider) <*> createApi) <| (sampleDomain, dataContext, authConfig)

            do! saveChangesAsync dataContext
            return { TenantId = tenant.Id
                     DomainPoolId = sampleDomain.PoolId.Value
                     TenantManagementDomainId = tenantManagementDomain.Id
                     TenantManagementApiId = tenantManagementApi.Id
                     TenantManagementApiIdentifier = tenantManagementApi.Identifier
                     TenantManagementApplicationId = tenantManagementApp.Id
                     TenantManagementApplicationClientId = tenantManagementApp.ClientId
                     DomainId = sampleDomain.Id
                     DomainManagementApiIdentifier = usersManagementApi.Identifier
                     DomainManagementApiId = usersManagementApi.Id
                     DomainManagementApplicationId = usersManagementApp.Id
                     DomainManagementApplicationClientId = usersManagementApp.ClientId
                     SampleApiIdentifier = sampleApi.Identifier
                     SampleApiId = sampleApi.Id
                     SampleApplicationId = sampeApp.Id
                     SampleApplicationClientId = sampeApp.ClientId }
        }
