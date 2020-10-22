namespace PRR.API.Routes.AuthSocial

open Giraffe

module SocialRoutes =

    let createRoutes () =
        choose [ PostAuthSocial.createRoute ()
                 GetAuthSocialCallback.createRoute () ]
