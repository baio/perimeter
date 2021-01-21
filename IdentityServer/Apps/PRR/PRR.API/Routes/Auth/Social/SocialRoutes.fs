namespace PRR.API.Routes.Auth.Social

open Giraffe

module Routes =

    let createRoutes () =
        choose [ PostAuthSocial.createRoute ()
                 GetAuthSocialCallback.createRoute () ]
