module PRR.API.Routes.Auth

open Common.Domain.Giraffe
open Common.Domain.Models
open Common.Utils.ReaderTask
open Giraffe
open PRR.API
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

    let private getRedirectUrl: GetResultUrlFun<_> =
        fun ctx ->
            function
            | Ok res ->
                sprintf "%s?code=%s&state=%s" res.RedirectUri res.Code res.State
            | Error ex ->
                printf "Error %O" ex
                let referer =
                    match ctx.Request.Headers.["Referer"] |> Seq.tryHead with
                    | Some r -> r
                    | None _ -> "http://fail"

                let concatError err =
                    sprintf "%s%serror=%s" referer
                        (if referer.Contains "?" then "&"
                         else "?") err
                match ex with
                | :? BadRequest ->
                    concatError "invalid_request"
                | :? UnAuthorized ->
                    concatError "unauthorized_client"
                | _ ->
                    concatError "server_error"

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

open Handlers

let createRoutes() =
    subRoute "/auth"
        (choose
            [ POST >=> choose
                           [ route "/sign-up/confirm" >=> signUpConfirmHandler
                             route "/sign-up" >=> signUpHandler
                             route "/sign-in" >=> signInHandler
                             route "/log-in" >=> signInTenantHandler
                             route "/log-in" >=> signInTenantHandler
                             route "/login" >=> logInHandler
                             route "/token" >=> logInTokenHandler
                             route "/refresh-token" >=> refreshTokenHandler
                             route "/reset-password/confirm" >=> resetPasswordConfirmHandler
                             route "/reset-password" >=> resetPasswordHandler ] ])
