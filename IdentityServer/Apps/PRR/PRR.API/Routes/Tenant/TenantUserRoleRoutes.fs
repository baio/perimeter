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
open PRR.Domain.Tenant.TenantUserRoles

[<AutoOpen>]
module private TenantUserRolesHandlers =

    let private dataContext = getDataContext |> ofReader

    let tenantId =
        bindUserClaimId
        >>= (fun id ctx ->
        ctx
        |> getDataContext
        |> Helpers.getTenantIdFromUserId id)

    let updateRolesHandler (forbidenRoles) =
        wrap (updateTenantRoles forbidenRoles <!> (doublet <!> tenantId <*> bindJsonAsync<PostLike>) <*> dataContext)

module TenantUserRole =

    let createRoutes() =
        POST >=> route "/tenant/users/roles" >=> choose
                                                     [ permissionOptGuard MANAGE_TENANT_ADMINS
                                                       >=> updateRolesHandler [ Seed.Roles.TenantOwner.Id ]
                                                       permissionOptGuard MANAGE_TENANT_DOMAINS
                                                       >=> updateRolesHandler
                                                               [ Seed.Roles.TenantOwner.Id
                                                                 Seed.Roles.TenantSuperAdmin.Id
                                                                 Seed.Roles.TenantAdmin.Id ]
                                                       RequestErrors.FORBIDDEN "User can't manage provided roles" ]
