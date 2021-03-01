namespace PRR.Domain.Auth.OpenIdConfiguration

[<AutoOpen>]
module OpenIdConfiguration =

    type OpenIdConfiguration =
        { JwksUri: string
          TokenEndpoint: string
          Issuer: string
          UserInfoEndpoint: string
          AuthorizationEndpoint: string
          DeviceAuthorizationEndpoint: string
          EndSessionEndpoint: string
          RbacUrl: string }

    let getOpenIdConfigurationJsonString (config: OpenIdConfiguration) =
        {| token_endpoint = config.TokenEndpoint
           token_endpoint_auth_methods_supported =
               [ "client_secret_post"
                 "private_key_jwt"
                 "client_secret_basic" ]
           jwks_uri = config.JwksUri
           response_modes_supported = [ "query"; "form_post" ]
           subject_types_supported = [ "pairwise" ]
           id_token_signing_alg_values_supported = [ "HS256"; "RS256" ]
           response_types_supported = [ "code" ]
           scopes_supported =
               [ "openid"
                 "profile"
                 "email"
                 "offline_access" ]
           issuer = config.Issuer
           request_uri_parameter_supported = false
           userinfo_endpoint = config.UserInfoEndpoint
           authorization_endpoint = config.AuthorizationEndpoint
           device_authorization_endpoint = config.DeviceAuthorizationEndpoint
           http_logout_supported = true
           frontchannel_logout_supported = true
           end_session_endpoint = config.EndSessionEndpoint
           claims_supported =
               [ "sub"
                 "iss"
                 "aud"
                 "exp"
                 "iat"
                 "nonce"
                 "name"
                 "email" ]
           tenant_region_scope = null
           rbac_url = config.RbacUrl |}
