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

module GetPostAuthorize =

    type GetPostMethod =
        | Get
        | Post

    let handler method next ctx =

        // https://auth0.com/docs/authorization/configure-silent-authentication

        let logger = getLogger ctx

        logger.LogInformation "Login handler"

        // The initial flow
        // 1. prompt=none and sso cookie not found -> set sso cookie, redirect to itself
        // 1.a we can't go directly to return_url?error=login_required of the tenant application, since we had to
        // set sso cookie on the identity provider web page
        // 2. after redirect to itself, prompt=none and sso cookie set in request, but sso cookie not exists in kv -> redirect back with login_required error
        // 3. tenant app callback hit with error=login_required, app must login user in regular way
        // 4. after this any user logins with prompt=none (until logout) should be success since cookie is set for identity provider web page

        task {
            let ssoCookie = bindCookie "sso" ctx

            let! data =
                ctx
                |> (match method with
                    | Post -> bindFormAsync<AuthorizeData>
                    | Get -> bindQueryString >> Task.FromResult)

            logger.LogDebug("Login parameters ${ssoCookie} ${data}", ssoCookie, data)

            match data.Prompt with
            | Some "none" ->
                match ssoCookie with
                | Some sso ->
                    logger.LogDebug("Prompt none and sso cookie found, use SSO handler")

                    let! (success, returnUrl) = AuthorizeSSOHandler.handler data ctx sso
                    if not success then
                        logger.LogDebug("SSO auth fails, remove old sso cookie since it could be corrupted", returnUrl)
                        ctx.Response.Cookies.Delete("sso")
                    ctx.Response.Redirect(returnUrl, true)
                    logger.LogDebug("Redirect to ${redirectTo}", returnUrl)
                    return! redirectTo false returnUrl next ctx
                | None ->
                    // sso cookie not found just redirect back to itself with sso cookie
                    // the redirect back must be to the idp server page for example prr.pw
                    logger.LogInformation("Prompt none and no SSO cookie, redirect back to itself with new sso cookie")

                    let hasher = getHash ctx
                    let token = hasher ()
                    // TODO : Secure = TRUE !!!
                    // TODO : Handle TOO many redirects when cookie couldn't be set
                    ctx.Response.Cookies.Append("sso", token, CookieOptions(HttpOnly = true, Secure = false))
                    let url = getRefererUrl ctx
                    // true here intentially, since with false browser will try find web page and not redirect request directly back to the server iniital request
                    // TODO : We need return page content here with SSO cookie included, browser seems arbitrary redirect to POST web page sometimes not to server
                    ctx.Response.Redirect(url, false)
                    // ctx.SetStatusCode(307)
                    logger.LogDebug("Redirect to ${redirectTo}", url)
                    return Some ctx
            | _ ->
                logger.LogInformation("Prompt not none use regular login handler")

                let! returnUrl = AuthorizeHandler.handler data ctx ssoCookie
                logger.LogInformation("Redirect to ${returnUrl}", returnUrl)
                return! redirectTo false returnUrl next ctx
        }
