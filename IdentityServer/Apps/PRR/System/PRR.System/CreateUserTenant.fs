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
open PRR.Domain.Tenant.Helpers

[<AutoOpen>]
module private CreateUserTenant =
    type Env =
        { GetDataContextProvider: GetDataContextProvider
          AuthConfig: AuthConfig
          AuthStringsProvider: AuthStringsProvider }


    /// Creates tenant, domain pool, domain, new app, new api and new users management api
    /// tenant -
    /// domain pool -
    /// app -
    /// api -
    /// users management api -
    let createUserTenant (env: Env) data =

        let authConfig' = env.AuthConfig

        let authConfig: PRR.Domain.Tenant.Models.AuthConfig =
            { IdTokenExpiresIn = authConfig'.IdTokenExpiresIn
              AccessTokenExpiresIn = authConfig'.AccessTokenExpiresIn
              RefreshTokenExpiresIn = authConfig'.ResetPasswordTokenExpiresIn }

        task {

            use dpr = env.GetDataContextProvider()

            let dataContext = dpr.DataContext

            let add' x = x |> add' dataContext

            let tenant =
                createTenant (sprintf "sample-tenant-%s" (env.AuthStringsProvider.ClientId())) data.UserId
                |> add'

            //
            let tenantManagementDomain =
                createTenantManagementDomain tenant |> add'

            let tenantManagementApp =
                createTenantManagementApp env.AuthStringsProvider authConfig tenantManagementDomain
                |> add'

            let tenantManagementApi =
                createTenantManagementApi authConfig tenantManagementDomain
                |> add'

            createDomainUserRole data.Email tenantManagementDomain Seed.Roles.TenantOwner.Id
            |> add'

            //
            let pool =
                createDomainPool tenant "sample-domain" |> add'

            let sampleDomain = createMainDomain pool |> add'

            let domainManagementApp =
                createDomainManagementApp env.AuthStringsProvider authConfig sampleDomain
                |> add'

            let domainManagementApi =
                createDomainManagementApi authConfig sampleDomain
                |> add'

            createDomainUserRole data.Email sampleDomain Seed.Roles.DomainOwner.Id
            |> add'

            //

            let sampleApp =
                createDomainApp env.AuthStringsProvider authConfig sampleDomain "sample-app"
                |> add'

            let sampleApi =
                createDomainApi authConfig sampleDomain "sample-api" "sample-api"
                |> add'

            do! saveChangesAsync dataContext

            return { TenantId = tenant.Id
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
