namespace PRR.API.Routes.Auth

[<AutoOpen>]
module AuthRoutes =

    open Giraffe

    let createRoutes () =
        subRoute
            "/auth"
            (choose [ route "/social" >=> PostAuthSocial.handler
                      GET
                      >=> route "/social/callback"
                      >=> GetAuthSocialCallback.handler
                      POST >=> route "/sign-up" >=> PostSignUp.handler
                      POST
                      >=> route "/sign-up/confirm"
                      >=> PostSignUpConfirm.handler
                      POST
                      >=> route "/reset-password"
                      >=> PostResetPassword.handler
                      POST
                      >=> route "/reset-password/confirm"
                      >=> PostResetPasswordConfirm.handler
                      POST
                      >=> route "/authorize"
                      >=> PostAuthorize.handler
                      POST
                      >=> route "/token"
                      >=> PostAuthorizeToken.handler
                      POST
                      >=> route "/refresh-token"
                      >=> PostRefreshToken.handler
                      GET
                      >=> route "/logout"
                      >=> LogOut.GetLogout.handler ])
