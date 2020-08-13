namespace Common.Domain.Utils

[<AutoOpen>]
module CommonExpressions =
    open Microsoft.EntityFrameworkCore
    open System.Linq

    let ilike (a: string) = <@ fun (b: string) -> EF.Functions.Like(b.ToLower(), ("%" + a.ToLower() + "%")) @>

    let in' (a: _ seq) =
        let a' = a |> Array.ofSeq
        <@ fun b -> a'.Contains(b) @>
