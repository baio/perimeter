namespace PRR.Domain.Tenant


open PRR.Domain.Models
open FSharp.Control.Tasks.V2.ContextInsensitive
open FSharpx.Reader
open PRR.Data.DataContext
open PRR.Data.Entities
open System
open System.Threading.Tasks
open PRR.Domain.Tenant
open PRR.Domain.Tenant.Helpers
open DataAvail.EntityFramework.Common
open DataAvail.ListQuery.Core
open DataAvail.ListQuery.EntityFramework

[<AutoOpen>]
module CreateUserTenant =

    type CreatedTenantInfo =
        { TenantId: int
          TenantManagementDomainId: int
          TenantManagementApiId: int
          TenantManagementApplicationId: int
          TenantManagementApiIdentifier: string
          TenantManagementApplicationClientId: string
          DomainPoolId: int
          DomainId: int
          DomainManagementApplicationId: int
          DomainManagementApplicationClientId: string
          DomainManagementApiId: int
          DomainManagementApiIdentifier: string
          SampleApiId: int
          SampleApiIdentifier: string
          SampleApplicationId: int
          SampleApplicationClientId: string }

    type Env =
        { DbDataContext: DbDataContext
          AuthConfig: AuthConfig
          AuthStringsGetter: IAuthStringsGetter }

    type UserTenantData = { UserId: UserId; Email: string }


    /// Creates tenant, domain pool, domain, new app, new api and new users management api
    /// tenant -
    /// domain pool -
    /// app -
    /// api -
    /// users management api -
    let createUserTenant (env: Env) (data: UserTenantData) =

        let authConfig = env.AuthConfig

        task {

            let dataContext = env.DbDataContext

            let add x = x |> add dataContext

            let add' x = x |> add' dataContext

            let tenant =
                createTenant (sprintf "sample-tenant-%s" (env.AuthStringsGetter.ClientId())) data.UserId
                |> add'

            //
            let tenantManagementDomain =
                createTenantManagementDomain env.AuthStringsGetter authConfig tenant
                |> add'

            let tenantManagementApp =
                createTenantManagementApp env.AuthStringsGetter authConfig tenantManagementDomain
                |> add'

            let tenantManagementApi =
                createTenantManagementApi env.AuthStringsGetter tenantManagementDomain
                |> add'

            createDomainUserRole data.Email tenantManagementDomain Seed.Roles.TenantOwner.Id
            |> add

            //
            let pool =
                createDomainPool tenant "sample-domain" "default-domain"
                |> add'

            let sampleDomain =
                createMainDomain env.AuthStringsGetter authConfig pool "dev"
                |> add'

            let domainManagementApp =
                createDomainManagementApp env.AuthStringsGetter authConfig sampleDomain
                |> add'

            let domainManagementApi =
                createDomainManagementApi env.AuthStringsGetter sampleDomain
                |> add'

            createDomainUserRole data.Email sampleDomain Seed.Roles.DomainOwner.Id
            |> add

            //

            let sampleApp =
                createDomainApp env.AuthStringsGetter authConfig sampleDomain "sample-app"
                |> add'

            let sampleApi =
                createDomainApi env.AuthStringsGetter sampleDomain "sample-api" "sample-api"
                |> add'

            do! saveChangesAsync dataContext

            return
                { TenantId = tenant.Id
                  DomainPoolId = sampleDomain.PoolId.Value
                  TenantManagementDomainId = tenantManagementDomain.Id
                  TenantManagementApiId = tenantManagementApi.Id
                  TenantManagementApiIdentifier = tenantManagementApi.Identifier
                  TenantManagementApplicationId = tenantManagementApp.Id
                  TenantManagementApplicationClientId = tenantManagementApp.ClientId
                  DomainId = sampleDomain.Id
                  DomainManagementApiIdentifier = domainManagementApi.Identifier
                  DomainManagementApiId = domainManagementApi.Id
                  DomainManagementApplicationId = domainManagementApp.Id
                  DomainManagementApplicationClientId = domainManagementApp.ClientId
                  SampleApiIdentifier = sampleApi.Identifier
                  SampleApiId = sampleApi.Id
                  SampleApplicationId = sampleApp.Id
                  SampleApplicationClientId = sampleApp.ClientId }
        }
