namespace PRR.API.Routes.Auth.Helpers

open System.Web
open Common.Domain.Models
open PRR.API
open System.Web
open Common.Domain.Models
open PRR.API
open PRR.Domain.Auth.LogIn
open Giraffe
open Common.Domain.Giraffe
open PRR.API.Routes
open FSharp.Control.Tasks.V2.ContextInsensitive

[<AutoOpen>]
module internal GetRedirectUrl =

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

    let private redirectUrl ctx err = concatQueryStringError (getRefererUrl ctx) err

    let getRedirectUrl isSSO redirUrl: GetResultUrlFun<_> =
        // TODO : Distinguish redir urls
        fun ctx ->
            function
            | Ok res -> sprintf "%s?code=%s&state=%s" res.RedirectUri res.Code res.State
            | Error ex ->
                printf "Error %O" ex
                match isSSO with
                | true -> "login_required"
                | false ->
                    match ex with
                    | :? BadRequest -> "invalid_request"
                    | :? UnAuthorized -> "unauthorized_client"
                    | _ -> "server_error"
                |> fun err -> if redirUrl <> null then concatQueryStringError redirUrl err else redirectUrl ctx err

    let private getLoginSuccessRedirectUrl (res: Result) =
        sprintf "%s?code=%s&state=%s" res.RedirectUri res.Code res.State

    let private getLoginErrorRedirectUrl (refererUrl, redirectUri) (ex: exn) =
        // RedirectUri could not be retrieved in some cases
        let redirectUri =
            if System.String.IsNullOrEmpty redirectUri then refererUrl else redirectUri

        match ex with
        | :? UnAuthorized as e -> concatErrorAndDescription refererUrl "unauthorized_client" e.Data0
        | :? BadRequest -> concatQueryStringError redirectUri "invalid_request"
        | _ -> concatQueryStringError redirectUri "server_error"

    let getRedirectUrl' urls =
        function
        | Ok res -> getLoginSuccessRedirectUrl res
        | Error (ex: exn) -> getLoginErrorRedirectUrl urls ex
