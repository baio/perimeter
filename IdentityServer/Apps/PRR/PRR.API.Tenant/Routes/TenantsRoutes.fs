namespace PRR.API.Tenant.Routes

open Giraffe
open PRR.API.Routes
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
          AuthConfig = config.TenantAuth }

    let createHandler =
        wrap
            (create <!> (ofReader getEnv)
             <*> (doublet <!> bindUserClaimId
                  <*> bindValidateJsonAsync validateData))

module Tenants =

    let createRoutes () =
        route "/tenants"
        >=> requiresAuth
        >=> POST
        >=> createHandler
