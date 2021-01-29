namespace PRR.API.Routes.Tenant

open Giraffe
open DataAvail.Common
open DataAvail.Common.ReaderTask
open DataAvail.Giraffe.Common
open PRR.API.Routes
open PRR.Data.DataContext
open PRR.Domain.Auth
open PRR.Domain.Auth.GetAudience
open PRR.Domain.Tenant.DomainUserRoles
open PRR.Domain.Tenant.TenantUserRoles
open DataAvail.ListQuery.Core

[<AutoOpen>]
module private TenantUserRolesHandlers =

    let private dataContext = getDataContext |> ofReader

    let updateRolesHandler tenantId (forbiddenRoles) =
        wrap
            (updateTenantRoles forbiddenRoles
             <!> (doublet tenantId <!> bindJsonAsync<PostLike>)
             <*> dataContext)

    let private bindListQuery =
        bindListQuery
            ((function
             | "email" -> Some SortField.UserEmail
             | _ -> None),
             (function
             | "email" -> Some FilterField.UserEmail
             | _ -> None))
        |> ofReader

    let getTenantAdminsList tenantId =
        wrap
            (getList
             <!> getDataContext'
             <*> (doublet tenantId <!> bindListQuery))

    let getOne tenantId email =
        wrap (getOne tenantId email <!> getDataContext')

    let remove tenantId email =
        wrap (remove tenantId email <!> getDataContext')

// TODO : User has permission to work with tenant !
module TenantUserRole =
    let createRoutes () =
        subRoutef "/tenants/%i/admins" (fun tenantId ->
            choose [ DELETE >=> routef "/%s" (remove tenantId)
                     GET >=> routef "/%s" (getOne tenantId)
                     GET
                     >=> route ""
                     >=> (getTenantAdminsList tenantId)
                     POST
                     >=> route ""
                     >=> choose [ permissionOptGuard MANAGE_TENANT_ADMINS
                                  >=> updateRolesHandler tenantId [ Seed.Roles.TenantOwner.Id ]
                                  permissionOptGuard MANAGE_TENANT_DOMAINS
                                  >=> updateRolesHandler
                                          tenantId
                                          [ Seed.Roles.TenantOwner.Id
                                            Seed.Roles.TenantSuperAdmin.Id
                                            Seed.Roles.TenantAdmin.Id ]
                                  RequestErrors.FORBIDDEN "User can't manage provided roles" ] ])
