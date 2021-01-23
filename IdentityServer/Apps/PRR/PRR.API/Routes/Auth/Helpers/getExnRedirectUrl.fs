namespace PRR.API.Routes.Auth.Helpers

open Common.Domain.Models
open Common.Domain.Giraffe

[<AutoOpen>]
module internal GetExnRedirectUrl =

    let getExnRedirectUrl (refererUrl, redirectUri) (ex: exn) =
        // RedirectUri could not be retrieved in some cases
        let redirectUri =
            if System.String.IsNullOrEmpty redirectUri then refererUrl else redirectUri

        match ex with
        | :? UnAuthorized as e -> concatErrorAndDescription refererUrl "unauthorized_client" e.Data0
        | :? BadRequest -> concatQueryStringError redirectUri "invalid_request"
        | _ -> concatQueryStringError redirectUri "server_error"
