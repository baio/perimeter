namespace PRR.API.Routes.Auth.Social

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
          SocialCallbackUrl = config.Auth.Social.CallbackUrl
          SocialCallbackExpiresIn = config.Auth.Social.CallbackExpiresIn
          PerimeterSocialClientIds =
              { Github = config.Auth.PerimeterSocialProviders.Github.ClientId
                Google = config.Auth.PerimeterSocialProviders.Google.ClientId
                Twitter = config.Auth.PerimeterSocialProviders.Twitter.ClientId } }

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


    let createRoute () = route "/social" >=> POST >=> handle
