namespace PRR.API.Routes.Tenant

open Common.Domain.Giraffe
open Common.Utils
open Common.Utils.ReaderTask
open Giraffe
open PRR.API.Routes
open PRR.Domain.Auth
open PRR.Domain.Auth.GetAudience
open PRR.Domain.Tenant.DomainPools
open PRR.Domain.Tenant.Models
open DataAvail.ListQuery.Core
[<AutoOpen>]
module private DomainPoolHandlers =

    let private dataContext = getDataContext |> ofReader

    let createHandler tenantId =
        wrap
            (create
             <!> (triplet tenantId
                  <!> bindUserClaimId
                  <*> bindValidateJsonAsync validatePostData)
             <*> ofReader (fun ctx ->
                     let config = getConfig ctx
                     { AuthStringsProvider = getAuthStringsGetter ctx
                       DataContext = getDataContext ctx
                       AuthConfig =
                           { AccessTokenSecret = config.Auth.Jwt.AccessTokenSecret
                             AccessTokenExpiresIn = config.Auth.Jwt.AccessTokenExpiresIn
                             IdTokenExpiresIn = config.Auth.Jwt.IdTokenExpiresIn
                             RefreshTokenExpiresIn = config.Auth.Jwt.RefreshTokenExpiresIn } }))

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

    let getList tenantId =
        wrap
            (getList
             <!> getDataContext'
             <*> (doublet tenantId <!> bindListQuery))

module DomainPool =

    let createRoutes () =
        subRoutef "/tenants/%i/domain-pools" (fun tenantId ->
            permissionGuard MANAGE_TENANT_DOMAINS
            >=> (choose [ POST >=> createHandler tenantId
                          routef "/%i" (fun domainPoolId ->
                              wrapAudienceGuard fromDomainPoolId domainPoolId
                              >=> choose [ PUT >=> updateHandler domainPoolId
                                           DELETE >=> removeHandler domainPoolId
                                           GET >=> getOne domainPoolId ])
                          GET >=> getList tenantId ]))
