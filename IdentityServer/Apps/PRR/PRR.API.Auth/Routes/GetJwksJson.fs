namespace PRR.API.Auth.Routes

module GetJwksJson =

    open PRR.Domain.Auth.WellKnown.GetJWKS
    open Giraffe
    open DataAvail.Giraffe.Common


    let handler' (tenant, domain, domainEnv) ctx =
        let env =
            { DataContext = (getDataContext ctx)
              Logger = (getLogger ctx) }

        let data =
            { Tenant = tenant
              Domain = domain
              Env = domainEnv }

        getJWKS env data

    let handler data = data |> handler' |> wrapHandlerOK
