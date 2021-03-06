namespace PRR.Domain.Auth.LogIn.AuthorizeDispatcher

[<AutoOpen>]
module private GetExnRedirectUrl =

    open DataAvail.Http.Exceptions
    open DataAvail.Common.Option

    let getExnRedirectUrl loginPageDomain data (ex: exn) =
        // RedirectUri could not be retrieved in some cases
        let (error, errorDescription) =
            match ex with
            | :? UnAuthorized as e -> "unauthorized_client", (ofOption e.Data0)
            | :? BadRequest -> "invalid_request", null
            | _ -> "server_error", null

        getRedirectAuthorizeUrl loginPageDomain data error errorDescription
