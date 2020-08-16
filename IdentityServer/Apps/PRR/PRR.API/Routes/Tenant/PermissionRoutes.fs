namespace PRR.API.Routes.Tenant

open Common.Domain.Giraffe
open Common.Utils
open Common.Utils.ReaderTask
open Giraffe
open PRR.API.Routes
open PRR.Domain.Auth.GetAudience
open PRR.Domain.Tenant.Permissions

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

module Permission =

    let createRoutes() =
        subRoutef "/tenant/apis/%i/permissions" (fun apiId ->
            (*wrapAudienceGuard fromApiId apiId >=>*)
            choose
                [ POST >=> (* permissionGuard MANAGE_PERMISSIONS >=> *) createHandler apiId
                  PUT >=> (* permissionGuard MANAGE_PERMISSIONS >=> *) routef "/%i" updateHandler
                  DELETE >=> (* permissionGuard MANAGE_PERMISSIONS >=> *) routef "/%i" removeHandler
                  GET >=> (*permissionGuard READ_PERMISSIONS >=>*) routef "/%i" getOne
                  GET >=> getList apiId ])
