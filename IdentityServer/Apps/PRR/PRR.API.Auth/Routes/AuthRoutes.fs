namespace PRR.API.Auth.Routes

[<AutoOpen>]
module AuthRoutes =

    open Giraffe

    let createRoutes () =
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
                  >=> GetPostAuthorize.handler GetPostAuthorize.GetPostMethod.Post
                  GET
                  >=> route "/authorize"
                  >=> GetPostAuthorize.handler GetPostAuthorize.GetPostMethod.Get
                  POST >=> route "/token" >=> PostToken.handler
                  GET >=> route "/logout" >=> GetLogout.handler
                  GET
                  >=> routef "/applications/%s" GetApplicationInfo.handler
                  PUT >=> route "/password" >=> PutPassword.handler
                  GET >=> route "/version" >=> Version.handler
                  GET
                  >=> routef "/issuers/%s/%s/%s/.well-known/openid-configuration" GetOpenIdConfiguration.handler
                  GET
                  >=> routef "/%s/.well-known/jwks.json" GetJwksJson.handler ])
