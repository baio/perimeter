namespace PRR.API.Routes.Auth

[<AutoOpen>]
module AuthRoutes =

    open Giraffe

    let createRoutes () =
        subRoute
            "/auth"
            (choose [ Social.Routes.createRoutes ()
                      SignUp.PostSignUp.createRoute ()
                      SignUpConfirm.PostSignUpConfirm.createRoute () ])
