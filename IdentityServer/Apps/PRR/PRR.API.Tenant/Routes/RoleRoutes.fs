namespace PRR.API.Routes.Tenant

open PRR.Domain.Models
open DataAvail.Common
open DataAvail.Common.ReaderTask
open DataAvail.Giraffe.Common
open Giraffe
open PRR.API.Routes
open PRR.Domain.Tenant.Roles
open DataAvail.ListQuery.Core

[<AutoOpen>]
module private RoleHandlers =

    let private dataContext = getDataContext |> ofReader

    let createHandler (domainId: int) =
        wrap
            (create
             <!> ((doublet domainId) <!> bindJsonAsync<PostLike>)
             <*> dataContext)

    let updateHandler (domainId: DomainId) (id: int) =
        wrap
            (update
             <!> ((doublet id)
                  <!> (doublet domainId <!> bindJsonAsync<PostLike>))
             <*> dataContext)

    let removeHandler (id: int) = wrap (remove id <!> dataContext)

    let getOne (id: int) = wrap (getOne id <!> dataContext)

    let bindListQuery =
        bindListQuery
            ((function
             | "name" -> Some SortField.Name
             | "dateCreated" -> Some SortField.DateCreated
             | _ -> None),
             (function
             | "text" -> Some FilterField.Text
             | _ -> None))
        |> ofReader

    let getList domainId =
        wrap
            (getList
             <!> getDataContext'
             <*> ((doublet domainId) <!> bindListQuery))


    let getAllUsersRoles domainId =
        wrap
            (getAllRoles (RoleType.User domainId)
             <!> getDataContext')

    let getAllAdminsRoles () =
        wrap
            (getAllRoles RoleType.DomainManagement
             <!> getDataContext')

    let getAllTenantAdminsRoles () =
        wrap
            (getAllRoles RoleType.TenantManagement
             <!> getDataContext')

module Role =

    let createRoutes () =
        choose [ GET
                 >=> route "/roles/admins"
                 >=> getAllAdminsRoles ()
                 GET
                 >=> route "/roles/tenant-admins"
                 >=> getAllTenantAdminsRoles ()
                 subRoutef "/domains/%i/roles" (fun domainId ->
                     wrapAudienceGuard fromDomainId domainId
                     >=> choose [ POST >=> createHandler domainId
                                  PUT >=> routef "/%i" (updateHandler domainId)
                                  DELETE >=> routef "/%i" removeHandler
                                  GET >=> routef "/%i" getOne
                                  GET
                                  >=> route "/users"
                                  >=> getAllUsersRoles domainId
                                  GET >=> getList domainId ]) ]
