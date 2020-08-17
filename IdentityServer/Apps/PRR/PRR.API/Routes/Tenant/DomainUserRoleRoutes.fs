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
        wrap (updateUsersRoles forbidenRoles <!> ((doublet domainId) <!> bindJsonAsync<PostLike>) <*> dataContext)

    let bindListQuery =
        bindListQuery
            ((function
             | "email" ->
                 Some SortField.UserEmail),
             (function
             | "email" ->
                 Some FilterField.UserEmail
             | _ -> None))
        |> ofReader

    let getList domainId =
        wrap (getList <!> getDataContext' <*> ((doublet domainId) <!> bindListQuery))

    let getOne (domainId, email) =
        wrap (getOne domainId email <!> getDataContext')

module DomainUserRole =

    let createRoutes() =
        choose
            [ GET >=> routef "/tenant/domains/%i/users/%s/roles" getOne
              GET >=> routef "/tenant/domains/%i/users/roles" getList
              POST >=> routef "/tenant/domains/%i/users/roles" (fun domainId ->
                           (* wrapAudienceGuard fromDomainId domainId *)
                           choose
                               [ updateRolesHandler [ Seed.Roles.DomainOwner.Id ] domainId
                                 (* permissionOptGuard MANAGE_DOMAIN_ADMINS >=> *)
                                 updateRolesHandler [ Seed.Roles.DomainSuperAdmin.Id; Seed.Roles.DomainOwner.Id ]
                                     domainId
                                 (* permissionOptGuard MANAGE_USERS >=> *)
                                 updateRolesHandler
                                     [ Seed.Roles.DomainAdmin.Id; Seed.Roles.DomainSuperAdmin.Id; Seed.Roles.DomainOwner.Id ]
                                     domainId
                                 RequestErrors.FORBIDDEN "User can't manage provided roles" ]) ]
