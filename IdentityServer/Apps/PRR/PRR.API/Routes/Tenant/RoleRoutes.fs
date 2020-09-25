namespace PRR.API.Routes.Tenant

open Common.Domain.Giraffe
open Common.Domain.Models
open Common.Utils
open Common.Utils.ReaderTask
open Giraffe
open PRR.API.Routes
open PRR.Domain.Auth.GetAudience
open PRR.Domain.Tenant.Roles

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
                 subRoutef "/tenant/domains/%i/roles" (fun domainId ->
                     wrapAudienceGuard fromDomainId domainId
                     >=> choose [ POST >=> createHandler domainId
                                  PUT >=> routef "/%i" (updateHandler domainId)
                                  DELETE >=> routef "/%i" removeHandler
                                  GET >=> routef "/%i" getOne
                                  GET
                                  >=> route "/users"
                                  >=> getAllUsersRoles domainId
                                  GET >=> getList domainId ]) ]
