namespace PRR.API.Routes.Auth

[<AutoOpen>]
module AuthRoutes =

    open Giraffe

    let createRoutes () =
        subRoute
            "/auth"
            (choose [ route "/social"
                      >=> Social.PostAuthSocial.createRoute ()
                      route "/social/callback"
                      >=> SocialCallback.GetAuthSocialCallback.createRoute ()
                      route "/sign-up"
                      >=> SignUp.PostSignUp.createRoute ()
                      route "/sign-up/confirm"
                      >=> SignUpConfirm.PostSignUpConfirm.createRoute ()
                      route "/reset-password"
                      >=> ResetPassword.PostResetPassword.createRoute ()
                      route "/reset-password/confirm"
                      >=> ResetPasswordConfirm.PostResetPasswordConfirm.createRoute ()
                      route "/authorize"
                      >=> Authorize.PostAuthorize.createRoute ()
                      route "/token"
                      >=> AuthorizeToken.PostAuthorizeToken.createRoute ()
                      route "/refresh-token"
                      >=> RefreshToken.PostRefreshToken.createRoute ()
                      route "/logout"
                      >=> LogOut.GetLogout.createRoute () ])
