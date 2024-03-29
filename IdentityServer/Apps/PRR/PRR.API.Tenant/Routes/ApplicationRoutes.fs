﻿namespace PRR.API.Tenant.Routes

open DataAvail.Common
open DataAvail.Common.ReaderTask
open DataAvail.Giraffe.Common
open Giraffe
open PRR.Domain.Tenant.Applications
open DataAvail.ListQuery.Core

[<AutoOpen>]
module private ApplicationHandlers =

    let private dataContext = getDataContext |> ofReader

    let createHandler domainId =
        wrap
            (create
             <!> ((fun ctx ->
                      let config = getConfig ctx
                      { RefreshTokenExpiresIn = config.TenantAuth.RefreshTokenExpiresIn
                        IdTokenExpiresIn = config.TenantAuth.IdTokenExpiresIn
                        AuthStringsProvider = getAuthStringsGetter ctx })
                  |> ofReader)
             <*> ((doublet domainId)
                  <!> bindValidateJsonAsync validatePost)
             <*> dataContext)

    let updateHandler domainId id =
        wrap
            (update
             <!> ((doublet id)
                  <!> (doublet domainId
                       <!> bindValidateJsonAsync validatePut))
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
             | "name" -> Some FilterField.Name
             | _ -> None))
        |> ofReader

    let getList domainId =
        wrap
            (getList
             <!> getDataContext'
             <*> ((doublet domainId) <!> bindListQuery))

module Application =

    let createRoutes () =
        subRoutef "/domains/%i/applications" (fun domainId ->
            permissionGuard MANAGE_DOMAIN
            >=> wrapAudienceGuard fromDomainId domainId
            >=> (choose [ POST >=> createHandler domainId
                          PUT >=> routef "/%i" (updateHandler domainId)
                          DELETE >=> routef "/%i" removeHandler
                          GET >=> routef "/%i" getOne
                          GET >=> getList domainId ]))
