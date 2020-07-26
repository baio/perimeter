namespace PRR.API.Routes.Tenant

open Common.Domain.Giraffe
open Common.Utils
open Common.Utils.ReaderTask
open Giraffe
open Giraffe.HttpStatusCodeHandlers
open PRR.API.Routes
open PRR.Data.DataContext
open PRR.Domain.Auth.GetAudience
open PRR.Domain.Tenant.DomainUserRoles

[<AutoOpen>]
module private DomainUserRolesHandlers =

    let private dataContext = getDataContext |> ofReader

    let updateRolesHandler (forbidenRoles) (domainId: int) =
        wrap (updateDomainRoles forbidenRoles <!> ((doublet domainId) <!> bindJsonAsync<PostLike>) <*> dataContext)

module DomainUserRole =

    let createRoutes() =
        POST
        >=> routef "/tenant/domains/%i/users/roles" (fun domainId ->
                wrapAudienceGuard fromDomainId domainId
                >=> choose
                        [ permissionOptGuard MANAGE_DOMAIN_SUPER_ADMINS
                          >=> updateRolesHandler [ Seed.Roles.DomainOwner.Id ] domainId
                          permissionOptGuard MANAGE_DOMAIN_ADMINS
                          >=> updateRolesHandler [ Seed.Roles.DomainSuperAdmin.Id; Seed.Roles.DomainOwner.Id ] domainId
                          permissionOptGuard MANAGE_USERS
                          >=> updateRolesHandler
                                  [ Seed.Roles.DomainAdmin.Id; Seed.Roles.DomainSuperAdmin.Id; Seed.Roles.DomainOwner.Id ]
                                  domainId
                          RequestErrors.FORBIDDEN "User can't manage provided roles" ])
