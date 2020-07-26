namespace Common.Domain.Utils

open Microsoft.FSharp.Linq

[<AutoOpen>]
module HandleOrder =

    open System.Linq
    open Common.Domain.Models
    open FSharpx.Reader

    let handleSortExpr (sort: Sort<'s>) fn q =

        let sortByExpr = <@ fun item -> (%(fn) sort.Field) item @>

        let asc =
            query {
                for i in q do
                    sortBy ((%sortByExpr) i)
            }

        let desc =
            query {
                for i in q do
                    sortByDescending ((%sortByExpr) i)
            }


        match sort.Order with
        | SortOrder.Asc -> asc
        | SortOrder.Desc -> desc

    let handleSort' sort fn q =
        match sort with
        | Some x -> handleSortExpr x fn q
        | None -> q

    let handleSort fn q =
        asks (fun (env: ListQuery<'s, 'f>) -> handleSort' env.Sort fn q)
