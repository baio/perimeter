namespace PRR.API.Routes.Tenant

open Common.Domain.Giraffe
open Common.Utils
open Common.Utils.ReaderTask
open Giraffe
open Giraffe.HttpStatusCodeHandlers
open Microsoft.FSharp.Linq
open PRR.API.Routes
open PRR.Data.DataContext
open PRR.Domain.Auth
open PRR.Domain.Auth.GetAudience
open PRR.Domain.Tenant.DomainUserRoles
open PRR.Domain.Tenant.TenantUserRoles

[<AutoOpen>]
module private TenantUserRolesHandlers =

    let private dataContext = getDataContext |> ofReader

    let updateRolesHandler (forbidenRoles) =
        wrap (updateTenantRoles forbidenRoles <!> (doublet <!> tenantId <*> bindJsonAsync<PostLike>) <*> dataContext)

    let bindListQuery =
        bindListQuery
            ((function
             | "email" ->
                 Some SortField.UserEmail
             | _ -> None),
             (function
             | "email" ->
                 Some FilterField.UserEmail
             | _ -> None))
        |> ofReader

    let getTenantAdminsList =
        wrap (getList <!> getDataContext' <*> (doublet <!> bindUserClaimId <*> bindListQuery))

    let getOne email =
        wrap (getOne email <!> bindUserClaimId <*> getDataContext')

    let remove email =
        wrap (remove email <!> bindUserClaimId <*> getDataContext')
module TenantUserRole =
    let createRoutes() =
        choose
            [ DELETE >=> routef "/tenant/admins/%s" remove
              GET >=> routef "/tenant/admins/%s" getOne
              GET >=> route "/tenant/admins" >=> getTenantAdminsList
              POST >=> route "/tenant/admins" >=> choose
                                                      [ permissionOptGuard MANAGE_TENANT_ADMINS
                                                        >=> updateRolesHandler [ Seed.Roles.TenantOwner.Id ]
                                                        permissionOptGuard MANAGE_TENANT_DOMAINS
                                                        >=> updateRolesHandler
                                                                [ Seed.Roles.TenantOwner.Id
                                                                  Seed.Roles.TenantSuperAdmin.Id
                                                                  Seed.Roles.TenantAdmin.Id ]
                                                        RequestErrors.FORBIDDEN "User can't manage provided roles" ] ]
