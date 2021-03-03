namespace PRR.Domain.Auth.LogIn.AuthorizeDispatcher

[<AutoOpen>]
module private GetExnRedirectUrl =

    open System.Web
    open DataAvail.Http.Exceptions
    open DataAvail.Common.Option
    open PRR.Domain.Auth.LogIn.Common

    (*
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
    *)

    let getExnRedirectUrl data (ex: exn) =
        // RedirectUri could not be retrieved in some cases
        let (error, errorDescription) =
            match ex with
            | :? UnAuthorized as e -> "unauthorized_client", (ofOption e.Data0)
            | :? BadRequest -> "invalid_request", null
            | _ -> "server_error", null

        getRedirectAuthorizeUrl data error errorDescription
