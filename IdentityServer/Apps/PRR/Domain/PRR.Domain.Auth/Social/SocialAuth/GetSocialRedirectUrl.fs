namespace PRR.Domain.Auth.Social.SocialAuth

open Common.Domain.Models

[<AutoOpen>]
module internal GetRedirectUrl =

    let getSocialRedirectUrl token callbackUri clientId =
        function
        | Github ->
            // https://docs.github.com/en/free-pro-team@latest/developers/apps/authorizing-oauth-apps
            sprintf
                "https://github.com/login/oauth/authorize?client_id=%s&redirect_uri=%s&state=%s"
                clientId
                callbackUri
                token
        | Google ->
            // https://developers.google.com/identity/protocols/oauth2/web-server
            sprintf
                "https://accounts.google.com/o/oauth2/v2/auth?client_id=%s&redirect_uri=%s&state=%s&response_type=code&scope=email profile openid"
                clientId
                callbackUri
                token
        | Twitter -> ""
