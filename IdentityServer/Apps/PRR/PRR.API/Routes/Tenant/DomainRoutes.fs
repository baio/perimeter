namespace PRR.API.Routes.Tenant

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
              { IdTokenExpiresIn = config.Jwt.IdTokenExpiresIn
                AccessTokenExpiresIn = config.Jwt.AccessTokenExpiresIn
                RefreshTokenExpiresIn = config.Jwt.RefreshTokenExpiresIn }
          AuthStringsProvider = authStringsProvider }

    let createHandler domainPoolId =
        wrap
            (create
             <!> ofReader (getEnv)
             <*> ((triplet domainPoolId)
                  <!> bindValidateJsonAsync validateData
                  <*> bindUserClaimId))

    let updateHandler id =
        wrap
            (update
             <!> ((doublet id) <!> bindJsonAsync<PostLike>)
             <*> dataContext)

    let removeHandler (id: DomainId) = wrap (remove id <!> dataContext)

    let getOne (id: DomainId) = wrap (getOne id <!> dataContext)

module Domain =

    let createRoutes () =
        subRoutef "/tenant/domain-pools/%i/domains" (fun domainPoolId ->
            permissionGuard MANAGE_TENANT_DOMAINS
            >=> wrapAudienceGuard fromDomainPoolId domainPoolId
            >=> (choose [ POST >=> createHandler domainPoolId
                          routef "/%i" (fun domainId ->
                              choose [ PUT >=> updateHandler domainId
                                       DELETE >=> removeHandler domainId
                                       GET >=> getOne domainId ]) ]))
