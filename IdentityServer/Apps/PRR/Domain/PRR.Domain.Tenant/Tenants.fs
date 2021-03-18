namespace PRR.Domain.Tenant

open PRR.Domain.Models
open FSharp.Control.Tasks.V2.ContextInsensitive
open Helpers
open PRR.Data.DataContext.Seed
open DataAvail.EntityFramework.Common
open DataAvail.Http.Exceptions
open DataAvail.Common.StringUtils
open PRR.Data.Entities

module Tenants =

    let catch =
        function
        | UniqueConstraintException "IX_DomainPools_TenantId_Name"
                                    (ConflictErrorCommon (sprintf "Domain pool with same name already exists"))
                                    ex -> raise ex
        | UniqueConstraintException "IX_Tenants_Name" (ConflictErrorField ("name", UNIQUE)) ex -> raise ex
        | ex -> raise ex

    [<CLIMutable>]
    type SandboxData =
        { DomainName: string
          EnvName: string
          ApiName: string
          AppName: string
          Permissions: string array }

    [<CLIMutable>]
    type PostLike =
        { Name: string
          Sandbox: SandboxData option }


    let validateData (data: PostLike) =
        [| (validateDomainName "name" data.Name) |]
        |> Array.choose id

    let createSandbox (env: Env) (tenant: Tenant) userEmail (data: SandboxData) =

        let add x = x |> add env.DataContext
        let add' x = x |> add' env.DataContext

        let domainPool =
            createDomainPool tenant data.DomainName (toLower data.DomainName)
            |> add'

        let domain =
            createMainDomain env.AuthStringsProvider env.AuthConfig domainPool data.EnvName
            |> add'

        // domain management
        createDomainManagementApp env.AuthStringsProvider env.AuthConfig domain
        |> add

        createDomainManagementApi env.AuthStringsProvider domain
        |> add

        createDomainUserRole userEmail domain Roles.DomainOwner.Id
        |> add

        // app
        let appData : Applications.PostLike =
            { Name = data.AppName
              GrantTypes =
                  [| GrantType.AuthorizationCodePKCE
                     GrantType.RefreshToken |]
                  |> Array.map (fun x -> System.Enum.GetName(typeof<GrantType>, x)) }


        let env' : Applications.CreateEnv =
            { RefreshTokenExpiresIn = env.AuthConfig.RefreshTokenExpiresIn
              IdTokenExpiresIn = env.AuthConfig.IdTokenExpiresIn
              AuthStringsProvider = env.AuthStringsProvider }

        Applications.create'' env' (domain.Id, appData)
        |> add

        // api
        let apiData : Apis.PostLike =
            { Name = data.ApiName
              Identifier = (toLower data.ApiName) }

        let apiTenantData : Apis.ParentData =
            { TenantName = tenant.Name
              DomainName = (toLower data.DomainName)
              EnvName = data.EnvName }

        let api =
            Apis.create'' (env.AuthStringsProvider.GetAudienceUri) (domain.Id, apiTenantData, apiData)
            |> add'

        // Permissions
        let permissionsData =
            data.Permissions
            |> Seq.map
                (fun name ->
                    { Name = name
                      Description = name
                      IsDefault = true }: Permissions.PostLike)

        permissionsData
        |> Seq.map (fun data -> Permissions.create'' (api.Id, data))
        |> addRange env.DataContext

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
                createTenantManagementDomain env.AuthStringsProvider env.AuthConfig tenant
                |> add'

            createTenantManagementApp env.AuthStringsProvider env.AuthConfig tenantManagementDomain
            |> add

            createTenantManagementApi env.AuthStringsProvider tenantManagementDomain
            |> add

            createDomainUserRole userEmail tenantManagementDomain Roles.TenantOwner.Id
            |> add

            printfn "222"

            try
                do! saveChangesAsync dataContext
                return tenant.Id
            with ex -> return catch ex
        }
