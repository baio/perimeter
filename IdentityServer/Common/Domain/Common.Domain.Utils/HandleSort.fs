namespace Common.Domain.Utils

open System
open System.Linq

[<AutoOpen>]
module HandleOrder =

    open Common.Domain.Models
    open FSharpx.Reader

    type SortExpr<'a> =
        | SortDate of Quotations.Expr<'a -> DateTime>
        | SortString of Quotations.Expr<'a -> string>

    let handleSortExpr (sort: Sort<'s>) fn (q: IQueryable<'a>) =

        // Look ! Type safety ! vomiting
        match sort.Order, (fn sort.Field) with
        | SortOrder.Asc, SortDate fn' ->
            query {
                for i in q do
                    sortBy ((%fn') i)
            }
        | SortOrder.Desc, SortDate fn' ->
            query {
                for i in q do
                    sortByDescending ((%fn') i)
            }
        | SortOrder.Asc, SortString fn' ->
            query {
                for i in q do
                    sortBy ((%fn') i)
            }
        | SortOrder.Desc, SortString fn' ->
            query {
                for i in q do
                    sortByDescending ((%fn') i)
            }



    let handleSort' sort fn q =
        match sort with
        | Some x -> handleSortExpr x fn q
        | None -> q

    let handleSort fn q =
        asks (fun (env: ListQuery<'s, 'f>) -> handleSort' env.Sort fn q)
