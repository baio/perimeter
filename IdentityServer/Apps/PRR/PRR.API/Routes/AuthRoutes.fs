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
        >>= ((bindSysQuery (RefreshToken.GetToken >> Queries.RefreshToken)) >> noneFails UnAuthorized)

    let refreshTokenHandler =

        sysWrapOK
            (refreshToken <!> ((fun ctx ->
                               { DataContext = getDataContext ctx
                                 HashProvider = getHash ctx
                                 JwtConfig = (getConfig ctx).Jwt })
                               |> ofReader) <*> (bindAuthorizationBearerHeader >> option2Task UnAuthorized)
             <*> bindRefreshTokenQuery)

    open PRR.Domain.Auth.SignUpConfirm

    let private bindSignUpTokenQuery =
        ((fun (x: Data) -> x.Token) <!> bindJsonAsync<Data>)
        >>= ((bindSysQuery (SignUpToken.GetToken >> Queries.SignUpToken)) >> noneFails UnAuthorized)

    let signUpConfirmHandler =
        sysWrap
            (signUpConfirm <!> ((fun ctx -> { DataContext = getDataContext ctx }) |> ofReader) <*> bindSignUpTokenQuery)


    open PRR.Domain.Auth.ResetPassword

    let resetPasswordHandler =
        sysWrap (resetPassword <!> (getDataContext |> ofReader) <*> bindJsonAsync<Data>)

    open PRR.Domain.Auth.ResetPasswordConfirm

    let private bindResetPasswordQuery =
        ((fun (x: Data) -> x.Token) <!> bindJsonAsync<Data>)
        >>= ((bindSysQuery (ResetPassword.GetToken >> Queries.ResetPassword)) >> noneFails UnAuthorized)

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

    let logInHandler =
        sysWrapOK (logIn <!> getLogInEnv <*> bindValidateJsonAsync validateData)

    open PRR.Domain.Auth.LogInToken

    let getLogInTokenEnv =
        ofReader (fun ctx ->
            { DataContext = getDataContext ctx
              HashProvider = getHash ctx
              JwtConfig = (getConfig ctx).Jwt })

    let private bindLogInCodeQuery =
        ((fun (x: Data) -> x.Code) <!> bindJsonAsync<Data>)
        >>= ((bindSysQuery (LogIn.GetCode >> Queries.LogIn)) >> noneFails UnAuthorized)

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
                             route "/login" >=> logInHandler
                             route "/token" >=> logInTokenHandler
                             route "/refresh-token" >=> refreshTokenHandler
                             route "/reset-password/confirm" >=> resetPasswordConfirmHandler
                             route "/reset-password" >=> resetPasswordHandler ] ])
