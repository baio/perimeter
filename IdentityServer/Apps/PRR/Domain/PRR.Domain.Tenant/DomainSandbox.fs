namespace PRR.Domain.Tenant

open PRR.Data.Entities

[<RequireQualifiedAccess>]
module DomainSandbox =

    open Helpers
    open PRR.Data.DataContext.Seed
    open DataAvail.EntityFramework.Common
    open DataAvail.Common.StringUtils

    type Data =
        { DomainName: string
          EnvName: string
          ApiName: string
          AppName: string
          Permissions: string array }

    let create (env: Env) (tenant: Tenant) userEmail (data: Data) =

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
        let appData: Applications.PostLike =
            { Name = data.AppName
              GrantTypes =
                  [| GrantType.AuthorizationCodePKCE
                     GrantType.RefreshToken |]
                  |> Array.map (fun x -> System.Enum.GetName(typeof<GrantType>, x)) }


        let env': Applications.CreateEnv =
            { RefreshTokenExpiresIn = env.AuthConfig.RefreshTokenExpiresIn
              IdTokenExpiresIn = env.AuthConfig.IdTokenExpiresIn
              AuthStringsProvider = env.AuthStringsProvider }

        Applications.create' env' (Entity domain, appData)
        |> add

        // api
        let apiData: Apis.PostLike =
            { Name = data.ApiName
              Identifier = (toLower data.ApiName) }

        let apiTenantData: Apis.ParentData =
            { TenantName = tenant.Name
              DomainName = (toLower data.DomainName)
              EnvName = data.EnvName }

        let api =
            Apis.create'' (env.AuthStringsProvider.GetAudienceUri) ((Entity domain), apiTenantData, apiData)
            |> add'

        // Permissions
        let permissionsData =
            data.Permissions
            |> Seq.map (fun name ->
                { Name = name
                  Description = name
                  IsDefault = true }: Permissions.PostLike)

        permissionsData
        |> Seq.map (fun data -> Permissions.create'' (Entity api, data))
        |> addRange env.DataContext

        ()
