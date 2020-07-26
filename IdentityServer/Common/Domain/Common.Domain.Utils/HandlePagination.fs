namespace Common.Domain.Utils

open Common.Domain.Models

[<AutoOpen>]
module HandlePager =

    open FSharpx.Reader

    let handlePagination' (pagination: Pager) q =
        let take' = pagination.Size
        let skip' = pagination.Index * take'
        query {
            for i in q do
                skip skip'
                take take'
        }


    let handlePagination q =
        asks (fun (env: ListQuery<'s, 'f>) -> handlePagination' env.Pager q)
