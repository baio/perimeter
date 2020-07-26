namespace Common.Domain.Utils

open System.Collections.Generic

[<AutoOpen>]
module HandleFilter =

    open Common.Domain.Models
    open FSharpx.Reader

    let composeFilterExpr filterFn (filters: Map<'x, string>) =

        let fn acc' (filter: KeyValuePair<'x, string>) =
            let fr = filterFn filter.Value filter.Key
            (match acc' with
             | Some acc ->
                 <@ fun user -> (%fr) user && (%acc) user @>
             | None ->
                 <@ fun user -> (%fr) user @>)
            |> Some
        filters |> Seq.fold fn None


    let handleFilter' filters fn q =
        match composeFilterExpr fn filters with
        | Some filterExpr ->
            query {
                for i in q do
                    where ((%filterExpr) i)
            }
        | None -> q

    let handleFilter fn q =
        asks (fun (env: ListQuery<'s, 'f>) -> handleFilter' env.Filters fn q)
