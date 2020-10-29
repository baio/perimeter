namespace PRR.API.Routes.AuthSocial

open Akkling
open Microsoft.AspNetCore.Http
open Giraffe
open PRR.API.Routes
open Common.Domain.Giraffe
open Common.Utils
open ReaderTask

open PRR.Domain.Auth.Social.SocialAuth
open PRR.Sys.Models.Social

module PostAuthSocial =

    let getContext (ctx: HttpContext): Env =
        let config = getConfig ctx
        { DataContext = getDataContext ctx
          HashProvider = getHash ctx
          SocialCallbackUrl = config.Social.CallbackUrl
          SocialCallbackExpiresIn = config.Social.CallbackExpiresIn
          PerimeterSocialClientIds = { Github = config.PerimeterSocialProviders.Github.ClientId } }

    let getParams =
        doublet
        <!> ofReader (getContext)
        <*> bindFormAsync

    let socialAuthResult (url, item) =
        fun ctx next ->
            let socialActor = (getSystemActors ctx).Social
            socialActor <! (SocialLoginAddCommand item)
            redirectTo false url next ctx

    let handle =
        (getParams
         >> TaskUtils.bind socialAuth
         >> TaskUtils.map socialAuthResult)
        |> wrapHandler


    let createRoute () = route "/auth/social" >=> POST >=> handle
