namespace PRR.Domain.Tenant

open Common.Domain.Utils
open Common.Domain.Models
open FSharp.Control.Tasks.V2.ContextInsensitive
open Helpers
open PRR.Data.DataContext.Seed

module Tenants =

    let catch =
        function
        | UniqueConstraintException "IX_DomainPools_TenantId_Name"
                                    (ConflictErrorCommon (sprintf "Domain pool with same name already exists"))
                                    ex -> raise ex
        | UniqueConstraintException "IX_Tenants_Name" (ConflictErrorField ("name", UNIQUE)) ex -> raise ex
        | ex -> raise ex

    type PostLike = { Name: string }


    let validateData (data: PostLike) =
        [| (validateDomainName "name" data.Name) |]
        |> Array.choose id

    let create (env: Env) ((userId, data): UserId * PostLike) =
        task {

            let dataContext = env.DataContext

            let add x = x |> add dataContext
            let add' x = x |> add' dataContext

            let! userEmail =
                query {
                    for user in dataContext.Users do
                        where (user.Id = userId)
                        select user.Email
                }
                |> toSingleAsync

            let tenant = createTenant data.Name userId |> add'

            // tenant management
            let tenantManagementDomain =
                createTenantManagementDomain tenant |> add'

            createTenantManagementApp env.AuthStringsProvider env.AuthConfig tenantManagementDomain
            |> add

            createTenantManagementApi env.AuthStringsProvider env.AuthConfig tenantManagementDomain
            |> add

            createDomainUserRole userEmail tenantManagementDomain Roles.TenantOwner.Id
            |> add

            // domain management
            let domainPool =
                createDomainPool tenant "Default" "default"
                |> add'

            let domain = createMainDomain domainPool |> add'

            createDomainManagementApp env.AuthStringsProvider env.AuthConfig domain
            |> add

            createDomainManagementApi env.AuthConfig domain
            |> add

            createDomainUserRole userEmail domain Roles.DomainOwner.Id
            |> add

            try
                do! saveChangesAsync dataContext
                return tenant.Id
            with ex -> return catch ex
        }
