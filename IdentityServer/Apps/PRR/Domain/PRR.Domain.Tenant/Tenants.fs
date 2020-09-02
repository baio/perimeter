namespace PRR.Domain.Tenant

open Common.Domain.Utils
open Common.Domain.Models
open Common.Domain.Utils.CRUD
open PRR.Data.DataContext
open PRR.Data.Entities
open FSharp.Control.Tasks.V2.ContextInsensitive
open Helpers

module Tenants =

    let catch =
        function
        | UniqueConstraintException "IX_DomainPools_TenantId_Name"
                                    (ConflictErrorCommon (sprintf "Domain pool with same name already exists"))
                                    ex -> raise ex
        | UniqueConstraintException "IX_Tenants_Name" (ConflictErrorField ("name", UNIQUE)) ex -> raise ex
        | ex -> raise ex

    type PostLike = { Name: string }


    let create (env: Env) ((userId, data): UserId * PostLike) =
        task {
            let dataContext = env.DataContext

            let add x = x |> add dataContext
            let add' x = x |> add' dataContext

            let tenant = createTenant data.Name userId |> add'

            // tenant management
            let tenantManagementDomain =
                createTenantManagementDomain tenant |> add'

            createTenantManagementApp env.AuthStringsProvider env.AuthConfig tenantManagementDomain
            |> add

            createTenantManagementApi tenantManagementDomain env.AuthConfig
            |> add

            // domain management
            let domainPool =
                createDomainPool tenant data.Name |> add'

            let domain = createMainDomain domainPool |> add'

            createDomainManagementApp env.AuthStringsProvider env.AuthConfig domain
            |> add

            createDomainManagementApi env.AuthConfig domain
            |> add

            try
                do! saveChangesAsync dataContext
                return tenant.Id
            with ex -> return catch ex
        }
