namespace Common.Domain.Giraffe

[<AutoOpen>]
module Headers =

    open Microsoft.AspNetCore.Http

    let bindHeader name (ctx: HttpContext) =
        let (f, v) = ctx.Request.Headers.TryGetValue(name)
        if f then Some v.[0]
        else None

    let bindAuthorizationHeader x = x |> bindHeader "Authorization"

    let bindAuthorizationBearerHeader x =
        x
        |> bindHeader "Authorization"
        |> Option.map (fun x -> x.Replace("Bearer", "").Trim())