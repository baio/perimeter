namespace PRR.API.Auth.Routes.Helpers

open System.Web
open DataAvail.Http.Exceptions

[<AutoOpen>]
module internal GetExnRedirectUrl =

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

    let getExnRedirectUrl (refererUrl, redirectUri) (ex: exn) =
        // RedirectUri could not be retrieved in some cases
        let redirectUri =
            if System.String.IsNullOrEmpty redirectUri then refererUrl else redirectUri

        match ex with
        | :? UnAuthorized as e -> concatErrorAndDescription refererUrl "unauthorized_client" e.Data0
        | :? BadRequest -> concatQueryStringError redirectUri "invalid_request"
        | _ -> concatQueryStringError redirectUri "server_error"
