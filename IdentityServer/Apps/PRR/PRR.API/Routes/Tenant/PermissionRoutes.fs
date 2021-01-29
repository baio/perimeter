namespace PRR.API.Routes.Tenant

open PRR.Domain.Models
open DataAvail.Common
open DataAvail.Common.ReaderTask
open DataAvail.Giraffe.Common
open Giraffe
open PRR.API.Routes
open PRR.Domain.Tenant
open PRR.Domain.Tenant.Permissions
open PRR.Domain.Auth.GetAudience
open DataAvail.ListQuery.Core

[<AutoOpen>]
module private PermissionHandlers =

    let private dataContext = getDataContext |> ofReader

    let createHandler (apiId: int) =
        wrap (create <!> ((doublet apiId) <!> bindJsonAsync<PostLike>) <*> dataContext)

    let updateHandler (id: int) =
        wrap (update <!> ((doublet id) <!> bindJsonAsync<PostLike>) <*> dataContext)

    let removeHandler (id: int) =
        wrap (remove id <!> dataContext)

    let getOne (id: int) =
        wrap (getOne id <!> dataContext)

    let bindListQuery =
        bindListQuery
            ((function
             | "name" ->
                 Some SortField.Name
             | "dateCreated" ->
                 Some SortField.DateCreated
             | _ -> None),
             (function
             | "text" ->
                 Some FilterField.Text
             | _ -> None))
        |> ofReader

    let getList domainId =
        wrap (getList <!> getDataContext' <*> ((doublet domainId) <!> bindListQuery))

    let getAllPermissions (domainId: DomainId) =
        wrap (Permissions.getAllDomainPermissions domainId <!> dataContext)

module Permission =

    let createRoutes() =
        choose [
            GET >=> routef "/tenant/domains/%i/permissions/all" getAllPermissions 
            subRoutef "/tenant/apis/%i/permissions" (fun apiId ->
                wrapAudienceGuard fromApiId apiId >=>
                choose
                    [ POST >=> permissionGuard MANAGE_PERMISSIONS >=>  createHandler apiId
                      PUT >=> permissionGuard MANAGE_PERMISSIONS >=> routef "/%i" updateHandler
                      DELETE >=> permissionGuard MANAGE_PERMISSIONS >=> routef "/%i" removeHandler
                      GET >=> permissionGuard READ_PERMISSIONS >=> routef "/%i" getOne                      
                      GET >=> permissionGuard READ_PERMISSIONS >=> getList apiId ])
        ]
