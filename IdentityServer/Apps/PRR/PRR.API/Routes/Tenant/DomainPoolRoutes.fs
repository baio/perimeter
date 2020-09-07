﻿namespace PRR.API.Routes.Tenant

open Common.Domain.Giraffe
open Common.Utils
open Common.Utils.ReaderTask
open Giraffe
open PRR.API.Routes
open PRR.Domain.Auth
open PRR.Domain.Auth.GetAudience
open PRR.Domain.Tenant.DomainPools
open PRR.Domain.Tenant.Models

[<AutoOpen>]
module private DomainPoolHandlers =

    let private dataContext = getDataContext |> ofReader

    let tenantId =
        bindUserClaimId
        >>= (fun id ctx ->
            ctx
            |> getDataContext
            |> Helpers.getTenantIdFromUserId id)

    let createHandler =
        wrap
            (create
             <!> (triplet
                  <!> bindUserClaimId
                  <*> tenantId
                  <*> bindValidateJsonAsync validatePostData)
             <*> ofReader (fun ctx ->
                     let config = getConfig ctx
                     { AuthStringsProvider = getAuthStringsProvider ctx
                       DataContext = getDataContext ctx
                       AuthConfig =
                           { AccessTokenSecret = config.Jwt.AccessTokenSecret
                             AccessTokenExpiresIn = config.Jwt.AccessTokenExpiresIn
                             IdTokenExpiresIn = config.Jwt.IdTokenExpiresIn
                             RefreshTokenExpiresIn = config.Jwt.RefreshTokenExpiresIn } }))

    let updateHandler id =
        wrap
            (update
             <!> ((doublet id)
                  <!> bindValidateJsonAsync validatePutData)
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

    let getList =
        wrap
            (getList
             <!> getDataContext'
             <*> (doublet <!> tenantId <*> bindListQuery))

module DomainPool =

    let createRoutes () =
        subRoute "/tenant/domain-pools" requiresAuth
        >=> permissionGuard MANAGE_TENANT_DOMAINS
        >=> (choose [ POST >=> createHandler
                      routef "/%i" (fun domainPoolId ->
                          wrapAudienceGuard fromDomainPoolId domainPoolId
                          >=> choose [ PUT >=> updateHandler domainPoolId
                                       DELETE >=> removeHandler domainPoolId
                                       GET >=> getOne domainPoolId ])
                      GET >=> getList ])
