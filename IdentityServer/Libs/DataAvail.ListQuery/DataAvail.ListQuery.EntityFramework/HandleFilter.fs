namespace DataAvail.ListQuery.EntityFramework

[<AutoOpen>]
module HandleFilter =

    open DataAvail.ListQuery.Core
    open System.Collections.Generic
    open FSharpx.Reader

    let composeFilterExpr filterFn (filters: Map<'x, string>) =

        let fn acc' (filter: KeyValuePair<'x, string>) =
            let fr = filterFn filter.Value filter.Key
            (match acc' with
             | Some acc ->
                 <@ fun x -> (%fr) x && (%acc) x @>
             | None ->
                 <@ fun x -> (%fr) x @>)
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
