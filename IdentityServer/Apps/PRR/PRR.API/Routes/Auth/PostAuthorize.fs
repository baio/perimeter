namespace PRR.API.Routes.Auth

open Giraffe
open Common.Domain.Giraffe
open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.AspNetCore.Http
open PRR.API.Routes
open PRR.Domain.Auth.LogInSSO
open Microsoft.Extensions.Logging
open PRR.API.Routes.Auth

module PostAuthorize =

    let handler next ctx =

        // https://auth0.com/docs/authorization/configure-silent-authentication

        let logger = getLogger ctx

        logger.LogInformation "Login handler"

        task {
            let ssoCookie = bindCookie "sso" ctx
            let! data = ctx.BindFormAsync<Data>()

            logger.LogInformation
                ("Login parameters ${@ssoCookie} ${@data}", ssoCookie, { data with Code_Challenge = "***" })

            match data.Prompt with
            | Some "none" ->
                match ssoCookie with
                | Some sso ->
                    logger.LogInformation
                        ("Prompt ${@prompt} and ${@ssoCookie}, use SSO handler", data.Prompt, ssoCookie)
                    let! returnUrl = PostLogInSSO.handler ctx sso
                    ctx.Response.Redirect(returnUrl, true)
                    return! redirectTo false returnUrl next ctx
                | None ->
                    // sso cookie not found just redirect back to itself with sso cookie
                    logger.LogInformation
                        ("Prompt ${@prompt} and no SSO cookie, redirect back to itself with new sso cookie", data.Prompt)
                    let hasher = getHash ctx
                    let token = hasher ()
                    // TODO : Secure = TRUE !!!
                    // TODO : Handle TOO many redirects when cookie couldn't be set
                    ctx.Response.Cookies.Append("sso", token, CookieOptions(HttpOnly = true, Secure = false))
                    let url = ctx.Request.HttpContext.GetRequestUrl()
                    logger.LogInformation("Using ${url} redirect", url)
                    ctx.Response.Redirect(url, true)
                    ctx.SetStatusCode(307)
                    return Some ctx
            | _ ->
                logger.LogInformation("No ${@prompt}, use regular login handler", data.Prompt)
                let! returnUrl = PostLogIn.handler ctx ssoCookie
                return! redirectTo false returnUrl next ctx
        }
