namespace FSharp.Mongo.ListQuery

open System.Collections.Generic
open MongoDB.Driver
open FSharp.MongoDB.Driver

[<AutoOpen>]
module HandleFilter =

    open Common.Domain.Models
    open FSharpx.Reader

    let composeFilterExpr filterFn (filters: Map<'x, string>) =

        let fn acc' (filter: KeyValuePair<'x, string>) =
            let fr = filterFn filter.Value filter.Key
            (match acc' with
             | Some acc -> <@ fun x -> (%fr) x && (%acc) x @>
             | None -> fr)
            |> Some

        filters |> Seq.fold fn None


    let handleFilter' filters fn (q: IMongoCollection<'a>) =
        match composeFilterExpr fn filters with
        | Some filterExpr ->
            let doc = bson filterExpr
            let filterDefinition = FilterDefinition.op_Implicit (doc)
            q.Find(filterDefinition)
        | None -> q.Find(FilterDefinition.Empty)

    let handleFilter fn q =
        asks (fun (env: ListQuery<'s, 'f>) -> handleFilter' env.Filters fn q)
