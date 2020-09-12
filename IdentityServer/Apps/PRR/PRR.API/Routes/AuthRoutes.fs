module PRR.API.Routes.Auth

open Common.Domain.Giraffe
open Common.Domain.Models
open Common.Utils
open Common.Utils.ReaderTask
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Identity
open Microsoft.Extensions.Primitives
open PRR.API
open PRR.Domain.Auth.LogInSSO
open PRR.Domain.Auth.LogOut
open PRR.System.Models

module private Handlers =

    open PRR.Domain.Auth.SignUp

    let signUpHandler =
        sysWrap
            (signUp
             <!> ((fun ctx ->
                      { DataContext = getDataContext ctx
                        HashProvider = getHash ctx })
                  |> ofReader)
             <*> bindValidateJsonAsync validateData)

    open PRR.Domain.Auth.SignUpConfirm

    let private bindSignUpTokenQuery =
        ((fun (x: Data) -> x.Token) <!> bindJsonAsync<Data>)
        >>= ((bindSysQuery (SignUpToken.GetToken >> Queries.SignUpToken))
             >> noneFails (UnAuthorized None))

    let signUpConfirmHandler =
        sysWrap
            (signUpConfirm
             <!> (ofReader
// Create default tenant for tests only                       
#if TEST                                          
                      (bindQueryString "skipCreateTenant"
                       >> function
                       | None -> true
                       | Some _ -> false)
#else
                        fun _ -> false
#endif
                      )
             <*> (ofReader (fun ctx -> { DataContext = getDataContext ctx }))
             <*> bindSignUpTokenQuery)

    open PRR.Domain.Auth.ResetPassword

    let resetPasswordHandler =
        sysWrap
            (resetPassword
             <!> (getDataContext |> ofReader)
             <*> bindJsonAsync<Data>)

    open PRR.Domain.Auth.ResetPasswordConfirm

    let private bindResetPasswordQuery =
        ((fun (x: Data) -> x.Token) <!> bindJsonAsync<Data>)
        >>= ((bindSysQuery (ResetPassword.GetToken >> Queries.ResetPassword))
             >> noneFails (UnAuthorized None))

    let resetPasswordConfirmHandler =
        sysWrap
            (resetPasswordConfirm
             <!> ((fun ctx ->
                      { DataContext = getDataContext ctx
                        PasswordSalter = getPasswordSalter ctx })
                  |> ofReader)
             <*> bindResetPasswordQuery
             <*> bindValidateJsonAsync validateData)

    open PRR.Domain.Auth.LogIn

    let getLogInEnv =
        ofReader (fun ctx ->
            let config = getConfig ctx
            { DataContext = getDataContext ctx
              PasswordSalter = getPasswordSalter ctx
              CodeGenerator = getHash ctx
              CodeExpiresIn = config.Jwt.CodeExpiresIn
              SSOExpiresIn = config.SSOCookieExpiresIn })

    let private concatError referer err =
        sprintf "%s%serror=%s" referer (if referer.Contains "?" then "&" else "?") err

    let private redirectUrl ctx err =
        let rurl =
            match bindHeader "Referer" ctx with
            | Some ref -> ref
            | None _ -> "http://referer-not-found"

        concatError rurl err

    let private getRedirectUrl isSSO redirUrl: GetResultUrlFun<_> =
        // TODO : Distinguish redir urls
        fun ctx ->
            function
            | Ok res -> sprintf "%s?code=%s&state=%s" res.RedirectUri res.Code res.State
            | Error ex ->
                printf "Error %O" ex
                match isSSO with
                | true -> "login_required"
                | false ->
                    match ex with
                    | :? BadRequest -> "invalid_request"
                    | :? UnAuthorized -> "unauthorized_client"
                    | _ -> "server_error"
                |> fun err -> if redirUrl <> null then concatError redirUrl err else redirectUrl ctx err

    let logInHandler sso redirUrl =
        sysWrapRedirect
            (getRedirectUrl false redirUrl)
            (logIn sso
             <!> getLogInEnv
             <*> bindValidateFormAsync validateData)

    open PRR.Domain.Auth.LogInToken

    let getLogInTokenEnv =
        ofReader (fun ctx ->
            { DataContext = getDataContext ctx
              HashProvider = getHash ctx
              Sha256Provider = getSHA256 ctx
              SSOCookieExpiresIn = (getConfig ctx).SSOCookieExpiresIn
              JwtConfig = (getConfig ctx).Jwt })

    let private bindLogInCodeQuery =
        ((fun (x: Data) -> x.Code) <!> bindJsonAsync<Data>)
        >>= ((bindSysQuery (LogIn.GetCode >> Queries.LogIn))
             >> noneFails (unAuthorized "Code not found"))

    let logInTokenHandler next ctx =
        sysWrapOK
            (logInToken
             <!> getLogInTokenEnv
             <*> bindLogInCodeQuery
             <*> bindValidateJsonAsync validateData)
            next
            ctx

    open PRR.Domain.Auth.RefreshToken

    let private bindRefreshTokenQuery =
        ((fun (x: Data) -> x.RefreshToken)
         <!> bindJsonAsync<Data>)
        >>= ((bindSysQuery (RefreshToken.GetToken >> Queries.RefreshToken))
             >> noneFails (UnAuthorized None))

    let refreshTokenHandler =
        sysWrapOK
            (refreshToken
             <!> getLogInTokenEnv
             <*> (bindAuthorizationBearerHeader
                  >> option2Task (UnAuthorized None))
             <*> bindRefreshTokenQuery)


    open PRR.Domain.Auth.LogInSSO

    let getLogInSSOEnv =
        ofReader (fun ctx ->
            { DataContext = getDataContext ctx
              CodeGenerator = getHash ctx
              CodeExpiresIn = (getConfig ctx).Jwt.CodeExpiresIn })

    let private bindLogSSOQuery sso =
        ofReader (fun _ -> sso)
        >>= ((bindSysQuery (SSO.GetCode >> Queries.SSO))
             >> noneFails (unAuthorized "Code not found"))

    let logInSSOHandler sso redirUrl =
        sysWrapRedirect
            (getRedirectUrl true redirUrl)
            (logInSSO
             <!> getLogInSSOEnv
             <*> bindValidateFormAsync validateData
             <*> bindLogSSOQuery sso)

    let authorizeHandler next (ctx: HttpContext) =
        // https://auth0.com/docs/authorization/configure-silent-authentication
        task {
            let ssoCookie = bindCookie "sso" ctx
            let! data = ctx.BindFormAsync<Data>()

            match data.Prompt with
            | Some "none" ->
                let errRedirectUrl =
                    sprintf "%s?error=login_required" data.Redirect_Uri

                let errRedirect () = redirectTo false errRedirectUrl next ctx
                match ssoCookie with
                | Some sso ->
                    try
                        return! logInSSOHandler sso data.Redirect_Uri next ctx
                    with _ ->
                        // TODO : Appropriate errors !
                        return! errRedirect ()
                | None ->
                    // sso cookie not found just redirect back to itself with sso cookie
                    let hasher = getHash ctx
                    let token = hasher ()
                    // TODO : Secure = TRUE !!!
                    // TODO : Handle TOO many redirects when cookie couldn't be set
                    ctx.Response.Cookies.Append("sso", token, CookieOptions(HttpOnly = true, Secure = false))
                    let url = ctx.Request.HttpContext.GetRequestUrl()
                    ctx.Response.Redirect(url, true)
                    ctx.SetStatusCode(307)
                    return Some ctx
            | _ -> return! logInHandler ssoCookie data.Redirect_Uri next ctx
        }

    open PRR.Domain.Auth.LogOut

    let logout data =
        logout
        <!> ofReader (fun ctx ->
                { DataContext = getDataContext ctx
                  AccessTokenSecret = (getConfig ctx).Jwt.AccessTokenSecret })
        <*> ofReader (fun _ -> data)

    let logoutHandler next (ctx: HttpContext) =
        ctx.Response.Cookies.Delete("sso")
        task {
            let returnUri =
                bindQueryString "return_uri" ctx
                |> Options.noneFails (unAuthorized "return_uri param is not found")

            let accessToken =
                bindQueryString "access_token" ctx
                |> Options.noneFails (unAuthorized "access_token param is not found")

            let data: Data =
                { ReturnUri = returnUri
                  AccessToken = accessToken }

            try
                let! res = logout data ctx
                let! (result, evt) = res
                sendEvent evt ctx
                return! redirectTo false result.ReturnUri next ctx
            with _ ->
                let url = redirectUrl ctx "return_uri"
                return! redirectTo false url next ctx
        }


open Handlers

let createRoutes () =
    subRoute
        "/auth"
        (choose [ GET >=> route "/logout" >=> logoutHandler
                  POST
                  >=> choose [ route "/login" >=> authorizeHandler
                               route "/sign-up/confirm" >=> signUpConfirmHandler
                               route "/sign-up" >=> signUpHandler
                               route "/token" >=> logInTokenHandler
                               route "/refresh-token" >=> refreshTokenHandler
                               route "/reset-password/confirm"
                               >=> resetPasswordConfirmHandler
                               route "/reset-password" >=> resetPasswordHandler ] ])
