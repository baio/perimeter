namespace PRR.API.Auth.Routes

module GetOpenIdConfiguration =

    open Giraffe
    open PRR.Domain.Auth.OpenIdConfiguration

    let handler (tenant, domain, env) next ctx =

        printfn "111 %s %s %s" tenant domain env

        let issuerBaseUrl = (getConfig ctx).IssuerBaseUrl

        let getPath str =
            sprintf
                "%sissuers/%s/%s/%s%s"
                issuerBaseUrl
                tenant
                domain
                env
                (if str = null then "" else (sprintf "/%s" str))

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
