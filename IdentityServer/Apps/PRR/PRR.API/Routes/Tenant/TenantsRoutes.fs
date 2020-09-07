namespace PRR.API.Routes.Tenants

open Common.Domain.Giraffe
open Common.Utils
open Common.Utils.ReaderTask
open Giraffe
open PRR.API.Routes
open PRR.Domain.Auth
open PRR.Domain.Tenant
open PRR.Domain.Tenant.Tenants


[<AutoOpen>]
module private TenantHandlers =

    let private getEnv ctx =
        let config = getConfig ctx
        { AuthStringsProvider = getAuthStringsProvider ctx
          DataContext = getDataContext ctx
          AuthConfig =
              { AccessTokenSecret = config.Jwt.AccessTokenSecret
                AccessTokenExpiresIn = config.Jwt.AccessTokenExpiresIn
                IdTokenExpiresIn = config.Jwt.IdTokenExpiresIn
                RefreshTokenExpiresIn = config.Jwt.RefreshTokenExpiresIn } }

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
