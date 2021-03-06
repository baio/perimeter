namespace PRR.API.Auth.Routes.Helpers

open DataAvail.Giraffe.Common


[<AutoOpen>]
module internal GetRefererUrl =

    let private getRefererHeader ctx = ctx |> bindHeader "Referer"

    let private getOriginHeader ctx = ctx |> bindHeader "Origin"

    let getRefererUrl ctx =
        ctx
        |> getRefererHeader
        |> function
        | Some x -> x
        | None -> "http://referer-not-found/"

    let getOriginUrl ctx =
        ctx
        |> getOriginHeader
        |> function
        | Some x -> x
        | None -> "http://origin-not-found/"
