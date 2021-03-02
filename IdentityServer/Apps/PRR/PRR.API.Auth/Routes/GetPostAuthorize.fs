namespace PRR.API.Auth.Routes

open System.Threading.Tasks
open Giraffe
open DataAvail.Giraffe.Common
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.AspNetCore.Http
open PRR.API.Auth.Routes.Helpers
open PRR.Domain.Auth.LogIn.AuthorizeSSO
open Microsoft.Extensions.Logging
open PRR.Domain.Auth.LogIn.Common
open DataAvail.Common
open PRR.Domain.Auth.LogIn.AuthorizeDispatcher

module GetPostAuthorize =

    let private getEnv ctx =

        let config = getConfig ctx

        { DataContext = getDataContext ctx
          PasswordSalter = getPasswordSalter ctx
          CodeGenerator = getHash ctx
          CodeExpiresIn = config.Auth.Jwt.CodeExpiresIn
          SSOExpiresIn = config.Auth.SSOCookieExpiresIn
          Logger = getLogger ctx
          KeyValueStorage = getKeyValueStorage ctx }

    type GetPostMethod =
        | Get
        | Post

    let private setSSOCookie ctx =
        let hasher = getHash ctx
        let hash = hasher ()
        ctx.Response.Cookies.Append("sso", hash, CookieOptions(HttpOnly = true, Secure = false))

    let private removeSSOCookie (ctx: HttpContext) = ctx.Response.Cookies.Delete("sso")

    let private updateSSOCookie ctx =
        function
        | SSOCookieExists -> ()
        | SSOCookieNotExists -> setSSOCookie ctx


    let handler method next ctx =

        // https://auth0.com/docs/authorization/configure-silent-authentication

        let logger = getLogger ctx

        logger.LogDebug "Authorize handler"

        // The initial flow
        // 1. prompt=none and sso cookie not found -> set sso cookie, redirect to itself
        // 1.a we can't go directly to return_url?error=login_required of the tenant application, since we had to
        // set sso cookie on the identity provider web page
        // 2. after redirect to itself, prompt=none and sso cookie set in request, but sso cookie not exists in kv -> redirect back with login_required error
        // 3. tenant app callback hit with error=login_required, app must login user in regular way
        // 4. after this any user logins with prompt=none (until logout) should be success since cookie is set for identity provider web page

        task {

            let env = getEnv ctx

            let ssoCookie = bindCookie "sso" ctx

            let! data =
                ctx
                |> (match method with
                    | Post -> bindFormAsync<AuthorizeData>
                    | Get -> bindQueryString >> Task.FromResult)

            logger.LogDebug("Authorize parameters {ssoCookie} {@data}", ssoCookie, data)

            let! result = authorizeDispatcher env ssoCookie data

            logger.LogDebug("Authorize result {@result}", result)

            let updateSSOCookie = updateSSOCookie ctx

            match result with
            | RedirectEmptyLoginPassword (url, ssoStatus) ->
                updateSSOCookie ssoStatus
                return! redirectTo false url next ctx
            | RedirectNoPromptSSO (url, ssoStatus) ->
                updateSSOCookie ssoStatus
                return! redirectTo false url next ctx
            | RedirectRegularSuccess (url) -> return! redirectTo false url next ctx
            | RedirectError (url) ->
                removeSSOCookie ctx
                return! redirectTo false url next ctx
        }
