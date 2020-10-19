namespace PRR.API.Routes.AuthSocialAuthRoutes

open System
open Common.Domain.Models.Models
open Akkling
open Microsoft.AspNetCore.Http
open PRR.API.Routes.Tenant
open PRR.Data.DataContext
open PRR.Sys.Social
open Common.Domain.Utils
open Giraffe
open PRR.API.Routes
open Common.Domain.Giraffe
open FSharp.Control.Tasks.V2.ContextInsensitive
open Common.Utils

open PRR.Domain.Auth.Social


module private Handlers =

    let private getContext (ctx: HttpContext): Env =
        { DataContext = getDataContext ctx
          HashProvider = getHash ctx }

    let private socialAuthResult (url, msg) =
        fun ctx next ->
            let socialActor = (getSystemActors ctx).Social
            socialActor <! msg
            redirectTo false url next ctx

    let wrapHandler fn next ctx =
        ctx
        |> fn
        |> TaskUtils.bind (fun hr -> hr ctx next)


    open FSharpx.Reader

    let handle =
        ((doublet <!> getContext <*> bindQueryString)
         >> socialAuth
         >> TaskUtils.map socialAuthResult)
        |> wrapHandler

[<AutoOpen>]
module Routes =
    let createRoute () =
        route "/auth/social" >=> GET >=> Handlers.handle
