namespace PRR.API.Auth.Routes

module GetOpenIdConfiguration =

    open Giraffe
    open PRR.Domain.Auth.OpenIdConfiguration
    open DataAvail.Common

    let handler (tenant, domain, env) next ctx =

        let issuerBaseUrl = (getConfig ctx).IssuerBaseUrl

        let getPath str =
            concatUrl [| issuerBaseUrl
                         "issuers"
                         tenant
                         domain
                         env
                         str |]

        let config: OpenIdConfiguration =
            { JwksUri = getPath ".well-known/jwks.json"
              TokenEndpoint = getPath "token"
              Issuer = getPath null
              UserInfoEndpoint = getPath "tbd"
              AuthorizationEndpoint = getPath "authorize"
              DeviceAuthorizationEndpoint = getPath "tbd"
              EndSessionEndpoint = getPath "logout"
              RbacUrl = "https://tenant-perimeter.pw" }

        let result = getOpenIdConfigurationJsonString config

        json result next ctx
