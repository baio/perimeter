namespace PRR.API.Routes.Authx.SocialAuthRoutes

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

open Common.Utils.ReaderTask

module SocialAuthRoutes =

    type Data =
        { ClientId: string
          SocialName: string }

    type Env =
        { DataContext: DbDataContext
          HashProvider: HashProvider }

    type SocialInfo =
        { ClientId: string
          Attributes: string seq
          Permissions: string seq }

    let getSocialConnectionInfo (dataContext: DbDataContext) clientId socialType =
        query {
            for app in dataContext.Applications do
                join sc in dataContext.SocialConnections on (app.DomainId = sc.DomainId)
                where (app.ClientId = clientId && sc.Name = socialType)
                select
                    { ClientId = sc.ClientId
                      Attributes = sc.Attributes
                      Permissions = sc.Permissions }
        }
        |> toSingleAsync


    let getRedirectUrl token callbackUri (info: SocialInfo) =
        function
        | Github ->
            // https://docs.github.com/en/free-pro-team@latest/developers/apps/authorizing-oauth-apps
            sprintf
                "https://github.com/login/oauth/authorize?client_id=%s&redirect_uri=%s&state=%s"
                info.ClientId
                callbackUri
                token

    let socialName2Type socialName =
        match socialName with
        | "github" -> Github
        | _ ->
            raise
                (sprintf "Social [%s] is not found" socialName
                 |> exn)

    let private socialAuth (env: Env, data: Data) =
        task {
            // get domain social info by type
            let socialType = socialName2Type data.SocialName

            let! info = getSocialConnectionInfo env.DataContext data.ClientId data.SocialName

            let token = env.HashProvider()

            let callbackUri = "http://localhost:4200/login/callback"

            let redirectUrl =
                getRedirectUrl token callbackUri info socialType

            let cmd =
                { Token = token
                  ClientId = info.ClientId
                  Type = socialType }
                |> SocialLoginAddCommand

            return (redirectUrl, cmd)
        }

    let getContext (ctx: HttpContext): Env =
        { DataContext = getDataContext ctx
          HashProvider = getHash ctx }

    let socialAuthSuccess (url, msg) =
        fun ctx next -> redirectTo false url next ctx

    let wrapHandler fn next ctx =
        ctx
        |> fn
        |> TaskUtils.bind (fun hr -> hr ctx next)

    let handleSocialAuth =
        wrapHandler
            ((doublet
              <!> (ofReader getContext)
              <*> bindJsonAsync)
             >> TaskUtils.bind socialAuth
             >> TaskUtils.map socialAuthSuccess)

    let createRoute () =
        route "/auth/social" >=> handleSocialAuth
