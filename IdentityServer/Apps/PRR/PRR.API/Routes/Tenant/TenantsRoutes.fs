namespace PRR.API.Routes.Tenants

open Giraffe
open PRR.API.Routes
open PRR.Domain.Auth
open PRR.Domain.Tenant
open PRR.Domain.Tenant.Tenants
open DataAvail.Giraffe.Common
open DataAvail.Common
open DataAvail.Common.ReaderTask

[<AutoOpen>]
module private TenantHandlers =

    let private getEnv ctx =
        let config = getConfig ctx
        { AuthStringsProvider = getAuthStringsGetter ctx
          DataContext = getDataContext ctx
          AuthConfig =
              { AccessTokenSecret = config.Auth.Jwt.AccessTokenSecret
                AccessTokenExpiresIn = config.Auth.Jwt.AccessTokenExpiresIn
                IdTokenExpiresIn = config.Auth.Jwt.IdTokenExpiresIn
                RefreshTokenExpiresIn = config.Auth.Jwt.RefreshTokenExpiresIn } }

    let createHandler =
        wrap
            (create
             <!> (ofReader getEnv)
             <*> (doublet
                  <!> bindUserClaimId
                  <*> bindValidateJsonAsync validateData))

module Tenants =

    let createRoutes () =
        route "/tenants"
        >=> requiresAuth
        >=> POST
        >=> createHandler
