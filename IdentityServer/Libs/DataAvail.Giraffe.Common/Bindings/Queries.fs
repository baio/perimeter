namespace DataAvail.Giraffe.Common

[<AutoOpen>]
module Queries =

    open Giraffe
    open Microsoft.AspNetCore.Http

    let bindQueryStringField name (ctx: HttpContext) =
        let (f, v) = ctx.Request.Query.TryGetValue(name)
        if f then v |> Seq.tryHead else None


    let bindQueryString<'a> (ctx: HttpContext) = ctx.BindQueryString<'a>()
