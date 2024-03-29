﻿namespace DataAvail.ListQuery.Mongo

open DataAvail.ListQuery.Core
open MongoDB.Driver
open FSharp.MongoDB.Driver

[<AutoOpen>]
module HandlePager =

    open FSharpx.Reader

    let handlePagination' (pagination: Pager) (q: IFindFluent<'a, 'b>) =
        let take' = pagination.Size
        let skip' = pagination.Index * take'
        q |> skip skip' |> limit take'

    let handlePagination q =
        asks (fun (env: ListQuery<'s, 'f>) -> handlePagination' env.Pager q)
