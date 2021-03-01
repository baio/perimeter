namespace PRR.API.Auth.Routes

module GetOpenIdConfiguration =

    open Giraffe
    open PRR.Domain.Auth.OpenIdConfiguration

    let handler next ctx =
        let config: OpenIdConfiguration =
            { JwksUri = "https://localhost:5001/api/auth/.well-known/jwks.json"
              TokenEndpoint = "https://localhost:5001/api/auth/token"
              Issuer = "https://localhost:5001/api/auth"
              UserInfoEndpoint = "https://tbd"
              AuthorizationEndpoint = "https://localhost:5001/api/auth/authorize"
              DeviceAuthorizationEndpoint = "https://tbd"
              EndSessionEndpoint = "https://localhost:5001/api/auth/logout"
              RbacUrl = "https://tenant-perimeter.pw" }

        let result = getOpenIdConfigurationJsonString config

        json result next ctx
