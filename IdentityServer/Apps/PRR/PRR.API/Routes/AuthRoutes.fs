module PRR.API.Routes.Auth

open Common.Domain.Giraffe
open Common.Domain.Models
open Common.Utils.ReaderTask
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Microsoft.AspNetCore.Http
open PRR.API
open PRR.Domain.Auth.LogInSSO
open PRR.System.Models

module private Handlers =

    open PRR.Domain.Auth.SignUp

    let signUpHandler =
        sysWrap
            (signUp <!> ((fun ctx ->
                         { DataContext = getDataContext ctx
                           HashProvider = getHash ctx })
                         |> ofReader)
             <*> bindValidateJsonAsync validateData)


    open PRR.Domain.Auth.SignIn

    let getSignInEnv =
        ofReader (fun ctx ->
            { DataContext = getDataContext ctx
              PasswordSalter = getPasswordSalter ctx
              HashProvider = getHash ctx
              JwtConfig = (getConfig ctx).Jwt })

    let signInHandler =
        sysWrapOK (signIn <!> getSignInEnv <*> bindJsonAsync)

    let signInTenantHandler =
        sysWrapOK (signInTenant <!> getSignInEnv <*> bindJsonAsync)


    open PRR.Domain.Auth.RefreshToken

    let private bindRefreshTokenQuery =
        ((fun (x: Data) -> x.RefreshToken) <!> bindJsonAsync<Data>)
        >>= ((bindSysQuery (RefreshToken.GetToken >> Queries.RefreshToken)) >> noneFails (UnAuthorized None))

    let refreshTokenHandler =

        sysWrapOK
            (refreshToken <!> ((fun ctx ->
                               { DataContext = getDataContext ctx
                                 HashProvider = getHash ctx
                                 JwtConfig = (getConfig ctx).Jwt })
                               |> ofReader) <*> (bindAuthorizationBearerHeader >> option2Task (UnAuthorized None))
             <*> bindRefreshTokenQuery)

    open PRR.Domain.Auth.SignUpConfirm

    let private bindSignUpTokenQuery =
        ((fun (x: Data) -> x.Token) <!> bindJsonAsync<Data>)
        >>= ((bindSysQuery (SignUpToken.GetToken >> Queries.SignUpToken)) >> noneFails (UnAuthorized None))

    let signUpConfirmHandler =
        sysWrap
            (signUpConfirm <!> ((fun ctx -> { DataContext = getDataContext ctx }) |> ofReader) <*> bindSignUpTokenQuery)

    open PRR.Domain.Auth.ResetPassword

    let resetPasswordHandler =
        sysWrap (resetPassword <!> (getDataContext |> ofReader) <*> bindJsonAsync<Data>)

    open PRR.Domain.Auth.ResetPasswordConfirm

    let private bindResetPasswordQuery =
        ((fun (x: Data) -> x.Token) <!> bindJsonAsync<Data>)
        >>= ((bindSysQuery (ResetPassword.GetToken >> Queries.ResetPassword)) >> noneFails (UnAuthorized None))

    let resetPasswordConfirmHandler =
        sysWrap
            (resetPasswordConfirm <!> ((fun ctx ->
                                       { DataContext = getDataContext ctx
                                         PasswordSalter = getPasswordSalter ctx })
                                       |> ofReader) <*> bindResetPasswordQuery <*> bindValidateJsonAsync validateData)

    open PRR.Domain.Auth.LogIn

    let getLogInEnv =
        ofReader (fun ctx ->
            { DataContext = getDataContext ctx
              PasswordSalter = getPasswordSalter ctx
              CodeGenerator = getHash ctx
              CodeExpiresIn = (getConfig ctx).Jwt.CodeExpiresIn })

    let private concatError referer err =
        sprintf "%s%serror=%s" referer
            (if referer.Contains "?" then "&"
             else "?") err

    let private redirectUrl ctx err =
        let rurl =
            match bindHeader "Referer" ctx with
            | Some ref -> ref
            | None _ -> "http://referer-not-found"
        concatError rurl err

    let private getRedirectUrl: GetResultUrlFun<_> =
        fun ctx ->
            function
            | Ok res ->
                sprintf "%s?code=%s&state=%s" res.RedirectUri res.Code res.State
            | Error ex ->
                printf "Error %O" ex
                match ex with
                | :? BadRequest ->
                    "invalid_request"
                | :? UnAuthorized ->
                    "unauthorized_client"
                | _ ->
                    "server_error"
                |> redirectUrl ctx

    let logInHandler =
        sysWrapRedirect getRedirectUrl (logIn <!> getLogInEnv <*> bindValidateFormAsync validateData)

    open PRR.Domain.Auth.LogInToken

    let getLogInTokenEnv =
        ofReader (fun ctx ->
            { DataContext = getDataContext ctx
              HashProvider = getHash ctx
              Sha256Provider = getSHA256 ctx
              JwtConfig = (getConfig ctx).Jwt })

    let private bindLogInCodeQuery =
        ((fun (x: Data) -> x.Code) <!> bindJsonAsync<Data>)
        >>= ((bindSysQuery (LogIn.GetCode >> Queries.LogIn)) >> noneFails (unAuthorized "Code not found"))

    let logInTokenHandler =
        sysWrapOK (logInToken <!> getLogInTokenEnv <*> bindLogInCodeQuery <*> bindValidateJsonAsync validateData)

    open PRR.Domain.Auth.LogInSSO

    let getLogInSSOEnv =
        ofReader (fun ctx ->
            { DataContext = getDataContext ctx
              CodeGenerator = getHash ctx
              CodeExpiresIn = (getConfig ctx).Jwt.CodeExpiresIn })

    let private bindLogSSOQuery sso =
        ofReader (fun _ -> sso)
        >>= ((bindSysQuery (SSO.GetCode >> Queries.SSO)) >> noneFails (unAuthorized "Code not found"))

    let logInSSOHandler sso =
        sysWrapRedirect getRedirectUrl
            (logInSSO <!> getLogInSSOEnv <*> bindValidateFormAsync validateData <*> bindLogSSOQuery sso)
    
    // TODO : Add cookie before login !
    let authorizeHandler next (ctx: HttpContext) =
        // https://auth0.com/docs/authorization/configure-silent-authentication
        task {
            let! promptData = bindJsonAsync<Data> ctx
            match promptData.Prompt with
            | Some "none" ->
                let errRedirectUrl = redirectUrl ctx "login_required"
                let errRedirect() = redirectTo false errRedirectUrl next ctx
                match bindCookie "sso" ctx with
                | Some sso ->
                    try
                        return! logInSSOHandler sso next ctx
                    with _ ->
                        // TODO : Appropriate errors !
                        return! errRedirect()
                | None ->
                    return! errRedirect()
            | None ->
                return! logInHandler next ctx
        }

open Handlers

let createRoutes() =
    subRoute "/auth"
        (choose
            [ POST >=> choose
                           [
                             // TODO : rename to login
                             route "/authorize" >=> authorizeHandler
                             route "/sign-up/confirm" >=> signUpConfirmHandler
                             route "/sign-up" >=> signUpHandler
                             // TODO  : remove
                             route "/sign-in" >=> signInHandler
                             // TODO  : remove
                             route "/log-in" >=> signInTenantHandler
                             // TODO  : remove
                             route "/log-in" >=> signInTenantHandler
                             // TODO  : remove
                             route "/login" >=> logInHandler
                             route "/token" >=> logInTokenHandler
                             route "/refresh-token" >=> refreshTokenHandler
                             route "/reset-password/confirm" >=> resetPasswordConfirmHandler
                             route "/reset-password" >=> resetPasswordHandler ] ])
