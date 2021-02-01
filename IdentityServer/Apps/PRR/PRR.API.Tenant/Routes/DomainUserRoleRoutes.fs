namespace PRR.API.Tenant.Routes

open DataAvail.Common
open DataAvail.Common.ReaderTask
open DataAvail.Giraffe.Common
open Giraffe
open PRR.API.Routes
open PRR.Data.DataContext
open PRR.Domain.Tenant.DomainUserRoles
open DataAvail.ListQuery.Core

[<AutoOpen>]
module private DomainUserRolesHandlers =

    let private dataContext = getDataContext |> ofReader

    let updateRolesHandler (forbidenRoles) (domainId: int) =
        wrap
            (updateUsersRoles forbidenRoles
             <!> ((doublet domainId)
                  <!> bindValidateAnnotatedJsonAsync<PostLike>)
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

    let getUsersList domainId =
        wrap
            (getList RoleType.User
             <!> getDataContext'
             <*> ((doublet domainId) <!> bindListQuery))

    let getDomainAdminsList domainId =
        wrap
            (getList RoleType.DomainManagement
             <!> getDataContext'
             <*> ((doublet domainId) <!> bindListQuery))

    let getOne (email, domainId) =
        wrap (getOne domainId email <!> getDataContext')

    let remove (domainId, email) =
        wrap (remove domainId email <!> getDataContext')

module DomainUserRole =

    let createRoutes () =
        choose [ DELETE
                 >=> routef "/domains/%i/users/%s" remove
                 GET
                 >=> routef "/domains/%i/users/%s" getOne
                 GET
                 >=> routef "/domains/%i/users" getUsersList
                 GET
                 >=> routef "/domains/%i/admins" getDomainAdminsList
                 POST
                 >=> routef "/domains/%i/users" (fun domainId ->
                         // check access token contains audience from the same domain
                         wrapAudienceGuard fromDomainId domainId
                         >=> choose [ permissionOptGuard MANAGE_DOMAIN_SUPER_ADMINS
                                      >=> updateRolesHandler [ Seed.Roles.DomainOwner.Id ] domainId
                                      permissionOptGuard MANAGE_DOMAIN_ADMINS
                                      >=> updateRolesHandler
                                              [ Seed.Roles.DomainSuperAdmin.Id
                                                Seed.Roles.DomainOwner.Id ]
                                              domainId
                                      permissionOptGuard MANAGE_USERS
                                      >=> updateRolesHandler
                                              [ Seed.Roles.DomainAdmin.Id
                                                Seed.Roles.DomainSuperAdmin.Id
                                                Seed.Roles.DomainOwner.Id ]
                                              domainId
                                      RequestErrors.FORBIDDEN "User can't manage provided roles" ]) ]
