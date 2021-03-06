namespace PRR.Domain.Auth.LogIn.AuthorizeDispatcher

open System
open DataAvail.Http.Exceptions
open PRR.Domain.Auth.Common

[<AutoOpen>]
module AuthorizeDispatcher =

    open PRR.Domain.Auth.LogIn.AuthorizeSSO
    open PRR.Domain.Auth.LogIn.Authorize
    open PRR.Domain.Auth.LogIn.Common
    open Microsoft.Extensions.Logging
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open DataAvail.Common

    let private tryAuthorizeAnotherDomain: AuthorizeDispatcher =
        fun env data ->
            let { AuthorizeEnv = env
                  LoginPageDomain = logInPageDomain } =
                env

            let { AuthorizeData = data } = data

            let logger = env.Logger

            logger.LogDebug("Authorize another domain data {@data}", data)

            task {

                let emailOrPasswordNotEmpty =
                    (isNotEmpty data.Email)
                    || (isNotEmpty data.Password)

                if emailOrPasswordNotEmpty then
                    raise
                        (BadRequest [| BadRequestCommonError
                                           "Email and password could be provided only through IDP form" |])

                match validateAuthorizeData false data with
                | Some ex ->
                    logger.LogWarning("Data validation failed {@ex}", ex)
                    raise ex
                | None -> ()

                return getRedirectAuthorizeUrl logInPageDomain data null null
            }

    let private tryAuthorizeIDPDomain: AuthorizeDispatcher =
        fun env data ->
            let { AuthorizeEnv = env
                  SetSSOCookie = setSSOCookie
                  LoginPageDomain = logInPageDomain } =
                env

            let { AuthorizeData = data
                  RefererUrl = refererUrl
                  SSOToken = ssoToken } =
                data

            let logger = env.Logger

            task {

                let isPromptNone =
                    match data.Prompt with
                    | Some "none" -> true
                    | _ -> false

                logger.LogDebug
                    ("Authorize IDP domain with data {@data} and sso {@sso} and refererUrl {refererUrl}",
                     data,
                     ssoToken,
                     refererUrl)

                // TODO : Basically we dont need any data besides sso in case there is sso cookie
                match validateAuthorizeData (not isPromptNone) data with
                | Some ex ->
                    logger.LogWarning("Data validation failed {@ex}", ex)
                    raise ex
                | None -> ()

                match isPromptNone with
                | true ->
                    match ssoToken with
                    | Some sso ->
                        logger.LogDebug("Prompt none and sso cookie found, use SSO handler")

                        let! result = authorizeSSO env sso data
                        return result
                    | None ->
                        logger.LogDebug("Prompt none and no SSO cookie, redirect back to idp page with new sso cookie")

                        let result =
                            getRedirectAuthorizeUrl logInPageDomain { data with Prompt = None } "login_required" null

                        setSSOCookie ()

                        return result
                | _ ->
                    logger.LogDebug("Prompt is not 'none' use regular login handler")

                    let! result = authorize env ssoToken data
                    return result
            }

    let private tryAuthorizeDispatcher: AuthorizeDispatcher =
        fun env data ->

            let { RefererUrl = refererUrl } = data

            let logger = env.AuthorizeEnv.Logger

            let loginPageUri = Uri env.LoginPageDomain

            let loginPageOrigin =
                loginPageUri.GetLeftPart UriPartial.Authority

            let isIDPDomain = refererUrl = loginPageOrigin

            logger.LogDebug("LoginPageDomain {isIDPDomain}", isIDPDomain)

            match isIDPDomain with
            | true -> tryAuthorizeIDPDomain env data
            | false -> tryAuthorizeAnotherDomain env data

    let authorizeDispatcher: AuthorizeDispatcher =
        fun env data ->
            env.AuthorizeEnv.Logger.LogDebug("AuthorizeDispatcher {@data}", data)

            task {
                try
                    return! tryAuthorizeDispatcher env data
                with ex ->
                    let redirectUrlError =
                        getExnRedirectUrl env.LoginPageDomain data.AuthorizeData ex

                    env.DeleteSSOCookie()

                    env.AuthorizeEnv.Logger.LogWarning
                        ("Redirect on error {@error} to {redirectUrlError}", ex, redirectUrlError)

                    return redirectUrlError
            }
