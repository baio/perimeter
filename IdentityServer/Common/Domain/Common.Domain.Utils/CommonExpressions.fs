namespace Common.Domain.Utils

[<AutoOpen>]
module CommonExpressions =    
    open Microsoft.EntityFrameworkCore
    open System.Collections.Generic
    open System.Linq
    
    let ilike (a: string) = <@ fun (b: string) -> EF.Functions.Like(a.ToLower(), "%" + b.ToLower() + "%") @>

    let in' (a: _ seq) = 
        let a' = a |> Array.ofSeq
        <@ fun b -> a'.Contains(b) @>