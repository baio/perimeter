﻿namespace PRR.API.Routes.Tenant

open Common.Domain.Giraffe
open Common.Domain.Models
open Common.Utils
open Common.Utils.ReaderTask
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Microsoft.AspNetCore.Http
open PRR.API.Routes
open PRR.Domain.Auth.GetAudience
open PRR.Domain.Tenant.Models
open PRR.Domain.Tenant.Domains

[<AutoOpen>]
module private DomainHandlers =

    let private dataContext = getDataContext |> ofReader

    let getEnv (ctx: HttpContext) =
        let config = getConfig ctx
        let authStringsProvider = getAuthStringsProvider ctx
        let dataContext = getDataContext ctx
        { DataContext = dataContext
          AuthConfig =
              { AccessTokenSecret = config.Jwt.AccessTokenSecret
                IdTokenExpiresIn = config.Jwt.IdTokenExpiresIn
                AccessTokenExpiresIn = config.Jwt.AccessTokenExpiresIn
                RefreshTokenExpiresIn = config.Jwt.RefreshTokenExpiresIn }
          AuthStringsProvider = authStringsProvider }

    let createHandler domainPoolId =
        wrap
            (create
             <!> ofReader (getEnv)
             <*> ofReader (getAuthStringsProvider)
             <*> ((triplet domainPoolId)
                  <!> bindValidateJsonAsync validatePostData
                  <*> bindUserClaimId))

    let updateHandler id =
        wrap
            (update
             <!> ((doublet id) <!> bindJsonAsync<PutLike>)
             <*> dataContext)

    let removeHandler (id: DomainId) = wrap (remove id <!> dataContext)

    let getOne (id: DomainId) = wrap (getOne id <!> dataContext)

    //
    open PRR.System.Views.LogInView

    let bindListQuery =
        bindListQuery
            ((function
             | "dateTime" -> Some SortField.DateTime
             | _ -> None),
             (function
             | "email" -> Some FilterField.Email
             | "appIdentifier" -> Some FilterField.AppIdentifier
             | "dateTime" -> Some FilterField.DateTime
             | _ -> None))
        |> ofReader

    let getLogIns (isManagement: bool) (domainId: DomainId) =
        wrap
            (getList
             <!> (ofReader getViewsReaderDb)
             <*> (triplet domainId isManagement <!> bindListQuery))

module Domain =

    let createRoutes () =
        subRoutef "/tenant/domain-pools/%i/domains" (fun domainPoolId ->
            choose [ POST
                     >=> permissionGuard MANAGE_TENANT_DOMAINS
                     >=> wrapAudienceGuard fromDomainPoolId domainPoolId
                     >=> createHandler domainPoolId
                     routef "/%i" (fun domainId ->
                         permissionGuard MANAGE_DOMAIN
                         >=> wrapAudienceGuard fromDomainId domainId
                         >=> choose [ PUT >=> updateHandler domainId
                                      DELETE >=> removeHandler domainId
                                      GET >=> getOne domainId
                                      GET
                                      >=> route "user-activities"
                                      >=> (getLogIns false domainId) ]) ])
