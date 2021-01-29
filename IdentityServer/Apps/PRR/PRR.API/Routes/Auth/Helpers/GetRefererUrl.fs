﻿namespace PRR.API.Routes.Auth.Helpers

open DataAvail.Giraffe.Common


[<AutoOpen>]
module internal GetRefererUrl =

    let private getRefererHeader ctx = ctx |> bindHeader "Referer"

    let getRefererUrl ctx =
        ctx
        |> getRefererHeader
        |> function
        | Some x -> x
        | None -> "http://referer-not-found"
