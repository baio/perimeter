namespace Common.Domain.Giraffe

[<AutoOpen>]
module Cookies =

    open Microsoft.AspNetCore.Http

    let bindCookie name (ctx: HttpContext) =
        let (f, v) = ctx.Request.Cookies.TryGetValue(name)
        if f then Some v
        else None
