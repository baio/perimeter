namespace PRR.Domain.Auth.Social.SocialAuth

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
