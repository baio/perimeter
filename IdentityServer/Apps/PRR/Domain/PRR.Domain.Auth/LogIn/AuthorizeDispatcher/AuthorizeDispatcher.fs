namespace PRR.Domain.Auth.LogIn.AuthorizeDispatcher

[<AutoOpen>]
module AuthorizeDispatcher =

    open PRR.Domain.Auth.LogIn.AuthorizeSSO
    open PRR.Domain.Auth.LogIn.Authorize
    open PRR.Domain.Auth.LogIn.Common
    open Microsoft.Extensions.Logging
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open DataAvail.Common

    let private tryAuthorizeDispatcher: AuthorizeDispatcher =
        fun env ssoCookie data ->
            let logger = env.Logger

            task {

                env.Logger.LogDebug("LogIn with data {@data} and sso {@sso}", data, ssoCookie)

                let emailPasswordEmpty =
                    (isEmpty data.Email) && (isEmpty data.Password)

                match validateAuthorizeData (not emailPasswordEmpty) data with
                | Some ex ->
                    env.Logger.LogWarning("Data validation failed {@ex}", ex)
                    raise ex
                | None -> ()

                let ssoCookieStatus =
                    match ssoCookie with
                    | Some _ -> SSOCookieExists
                    | None -> SSOCookieNotExists

                if emailPasswordEmpty then
                    env.Logger.LogDebug("Email and password are empty redirect to login page")
                    let result = getRedirectAuthorizeUrl data
                    return RedirectEmptyLoginPassword(result, ssoCookieStatus)
                else
                    match data.Prompt with
                    | Some "none" ->
                        match ssoCookie with
                        | Some sso ->
                            logger.LogDebug("Prompt none and sso cookie found, use SSO handler")

                            let! result = authorizeSSO env sso data
                            return RedirectNoPromptSSO(result, SSOCookieExists)
                        | None ->
                            // the redirect must be to the idp server page for example prr.pw
                            logger.LogDebug
                                ("Prompt none and no SSO cookie, redirect back to idp page with new sso cookie")

                            let result = getRedirectAuthorizeUrl data
                            return RedirectNoPromptSSO(result, SSOCookieNotExists)
                    | _ ->
                        logger.LogDebug("Prompt is not none use regular login handler")

                        let! result = authorize env ssoCookie data
                        return RedirectRegularSuccess result
            }

    let authorizeDispatcher: AuthorizeDispatcher =
        fun env ssoCookie data ->
            task {
                try
                    return! tryAuthorizeDispatcher env ssoCookie data
                with ex ->
                    let redirectUrlError =
                        getExnRedirectUrl "https://localhost:4201" ex

                    env.Logger.LogWarning("Redirect on ${@error} to ${redirectUrlError}", ex, redirectUrlError)

                    return RedirectError redirectUrlError
            }
