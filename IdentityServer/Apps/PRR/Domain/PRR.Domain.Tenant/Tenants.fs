namespace PRR.Domain.Tenant

open PRR.Domain.Models
open FSharp.Control.Tasks.V2.ContextInsensitive
open Helpers
open PRR.Data.DataContext.Seed
open DataAvail.EntityFramework.Common
open DataAvail.Http.Exceptions

module Tenants =

    let catch =
        function
        | UniqueConstraintException "IX_DomainPools_TenantId_Name"
                                    (ConflictErrorCommon (sprintf "Domain pool with same name already exists"))
                                    ex -> raise ex
        | UniqueConstraintException "IX_Tenants_Name" (ConflictErrorField ("name", UNIQUE)) ex -> raise ex
        | ex -> raise ex

    type PostLike =
        { Name: string
          Sandbox: DomainSandbox.Data option }

    let validateData (data: PostLike) =
        [| (validateDomainName "name" data.Name) |]
        |> Array.choose id


    let create (env: Env) ((userId, data): UserId * PostLike) =
        task {

            let dataContext = env.DataContext

            let add  x = x |> add dataContext
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
                createTenantManagementDomain env.AuthStringsProvider env.AuthConfig tenant
                |> add'

            createTenantManagementApp env.AuthStringsProvider env.AuthConfig tenantManagementDomain
            |> add

            createTenantManagementApi env.AuthStringsProvider tenantManagementDomain
            |> add

            createDomainUserRole userEmail tenantManagementDomain Roles.TenantOwner.Id
            |> add

            // sandbox
            match data.Sandbox with
            | Some sandbox -> DomainSandbox.create env tenant userEmail sandbox
            | None -> ()

            try
                do! saveChangesAsync dataContext
                return tenant.Id
            with ex -> return catch ex
        }
