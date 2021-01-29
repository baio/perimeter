﻿namespace PRR.API.Routes.Tenant

open Common.Domain.Giraffe
open Common.Utils
open Common.Utils.ReaderTask
open Giraffe
open PRR.API.Routes
open PRR.Domain.Tenant.Apis
open PRR.Domain.Auth.GetAudience
open DataAvail.ListQuery.Core

[<AutoOpen>]
module private ApiHandlers =

    let private dataContext = getDataContext |> ofReader

    let getEnv ctx =
        let config = getConfig ctx
        let authStringsProvider = getAuthStringsGetter ctx
        { AccessTokenExpiresIn = config.Auth.Jwt.AccessTokenExpiresIn
          HS256SigningSecret = authStringsProvider.HS256SigningSecret }

    let createHandler domainId =

        wrap
            (create
             <!> (ofReader getEnv)
             <*> ((doublet domainId)
                  <!> bindValidateJsonAsync validatePostData)
             <*> dataContext)

    let updateHandler domainId id =
        wrap
            (update
             <!> ((doublet id)
                  <!> (doublet domainId
                       <!> bindValidateJsonAsync validatePutData))
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

module Api =

    let createRoutes () =
        subRoutef "/tenant/domains/%i/apis" (fun domainId ->
            permissionGuard MANAGE_DOMAIN
            >=> wrapAudienceGuard fromDomainId domainId
            >=> (choose [ POST >=> createHandler domainId
                          PUT >=> routef "/%i" (updateHandler domainId)
                          DELETE >=> routef "/%i" removeHandler
                          GET >=> routef "/%i" getOne
                          GET >=> getList domainId ]))
