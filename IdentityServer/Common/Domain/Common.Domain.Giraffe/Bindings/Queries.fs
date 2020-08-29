namespace Common.Domain.Giraffe

[<AutoOpen>]
module Queries =

    open Microsoft.AspNetCore.Http

    let bindQueryString name (ctx: HttpContext) =
        let (f, v) = ctx.Request.Query.TryGetValue(name)
        if f then v |> Seq.tryHead
        else None
