namespace PRR.API.Routes.Tenant

open Common.Domain.Giraffe
open Common.Utils
open Common.Utils.ReaderTask
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open PRR.API.Routes
open PRR.Domain.Auth.GetAudience
open PRR.Domain.Tenant.Domains

[<AutoOpen>]
module private DomainHandlers =

    let private dataContext = getDataContext |> ofReader

    let createHandler domainPoolId =
        wrap (create <!> ((doublet domainPoolId) <!> bindJsonAsync<PostLike>) <*> dataContext)

    let updateHandler id =
        wrap (update <!> ((doublet id) <!> bindJsonAsync<PostLike>) <*> dataContext)

    let removeHandler (id: int) =
        wrap (remove id <!> dataContext)

    let getOne (id: int) =
        wrap (getOne id <!> dataContext)

module Domain =

    let createRoutes() =
        subRoutef "/tenant/domain-pools/%i/domains" (fun domianPoolId ->
            permissionGuard MANAGE_TENANT_DOMAINS >=> wrapAudienceGuard fromDomainPoolId domianPoolId
            >=> (choose
                     [ POST >=> createHandler domianPoolId
                       routef "/%i" (fun domainId ->
                           choose
                               [ PUT >=> updateHandler domainId
                                 DELETE >=> removeHandler domainId
                                 GET >=> getOne domainId ]) ]))
