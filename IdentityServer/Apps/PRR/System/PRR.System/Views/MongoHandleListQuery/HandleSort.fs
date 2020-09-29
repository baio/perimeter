namespace FSharp.Mongo.ListQuery

open System
open System.Linq
open MongoDB.Driver
open FSharp.MongoDB.Driver

[<AutoOpen>]
module HandleOrder =

    open Common.Domain.Models
    open FSharpx.Reader

    type SortExpr<'a> =
        | SortDate of Quotations.Expr<'a -> DateTime>
        | SortString of Quotations.Expr<'a -> string>

    let private handleSortExpr (sort: Sort<'s>) fn (q: IFindFluent<'a, 'b>) =

        match sort.Order, (fn sort.Field) with
        | SortOrder.Asc, SortDate fn' -> bsonOrder fn' BsonOrderAsc
        | SortOrder.Desc, SortDate fn' -> bsonOrder fn' BsonOrderDesc
        | SortOrder.Asc, SortString fn' -> bsonOrder fn' BsonOrderAsc
        | SortOrder.Desc, SortString fn' -> bsonOrder fn' BsonOrderDesc

    let private handleSort' sort fn q =
        match sort with
        | Some x ->
            let sortDoc = handleSortExpr x fn q
            q.Sort(SortDefinition.op_Implicit (sortDoc))
        | None -> q

    let handleSort fn q =
        asks (fun (env: ListQuery<'s, 'f>) -> handleSort' env.Sort fn q)
