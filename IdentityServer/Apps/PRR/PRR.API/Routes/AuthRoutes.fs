module PRR.API.Routes._Auth

open System.Web
open Common.Domain.Giraffe
open Common.Domain.Models
open Common.Utils
open Common.Utils.ReaderTask
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Microsoft.AspNetCore.Http
open PRR.API
open PRR.Domain.Auth.LogInSSO
open PRR.Domain.Auth.LogOut
open PRR.System.Models

module private Handlers =

    let private getRefererHeader ctx = ctx |> bindHeader "Referer"

    let getRefererUrl ctx =
        ctx
        |> getRefererHeader
        |> function
        | Some x -> x
        | None -> "http://referer-not-found"

    let concatQueryString (url: string) key v =
        let kv = sprintf "%s=%s" key v

        if kv |> (HttpUtility.UrlDecode url).Contains |> not
        then sprintf "%s%s%s" url (if url.Contains "?" then "&" else "?") kv
        else url

    let concatQueryStringError url = concatQueryString url "error"

    let private concatErrorDescription url =
        concatQueryString url "error_description"

    let concatErrorAndDescription url err =
        function
        | Some descr -> concatErrorDescription (concatQueryStringError url err) descr
        | None -> err |> concatQueryStringError url

    let private redirectUrl ctx err =
        concatQueryStringError (getRefererUrl ctx) err



    open PRR.Domain.Auth.LogOut

    let logout data =
        logout
        <!> ofReader
                (fun ctx ->
                    { DataContext = getDataContext ctx
                      AccessTokenSecret = (getConfig ctx).Auth.Jwt.AccessTokenSecret })
        <*> ofReader (fun _ -> data)

    let logoutHandler next (ctx: HttpContext) =
        ctx.Response.Cookies.Delete("sso")

        task {
            let returnUri =
                bindQueryStringField "return_uri" ctx
                |> Options.noneFails (unAuthorized "return_uri param is not found")

            let accessToken =
                bindQueryStringField "access_token" ctx
                |> Options.noneFails (unAuthorized "access_token param is not found")

            let data: Data =
                { ReturnUri = returnUri
                  AccessToken = accessToken }

            try
                let! res = logout data ctx
                let! (result, evt) = res
                sendEvent evt ctx
                return! redirectTo false result.ReturnUri next ctx
            with _ ->
                let url = redirectUrl ctx "return_uri"
                return! redirectTo false url next ctx
        }


open Handlers

let createRoutes () =
    subRoute "/auth" (choose [ GET >=> route "/logout" >=> logoutHandler ])
