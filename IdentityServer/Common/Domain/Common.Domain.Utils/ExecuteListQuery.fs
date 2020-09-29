namespace Common.Domain.Models

open System
open System.Threading.Tasks
open FSharp.Control.Tasks.V2.ContextInsensitive
open Common.Domain.Utils

[<AutoOpen>]
module ExecuteListQuery =

    let executeListQuery' q =

        task {
            // can't run in parallel https://go.microsoft.com/fwlink/?linkid=2097913.))))"
            let! list = q |> toListAsync
            let! count = q |> countAsync
            return (list, count)
        }

    let executeGroupByQuery' fn qcount q =

        task {
            // can't run in parallel https://go.microsoft.com/fwlink/?linkid=2097913.))))"
            let! list = q |> groupByAsync fn
            let! count = qcount |> countAsync
            return (list, count)
        }

    let mapResponse (prms: ListQuery<_, _>) (items: 'a seq, count: int) =
        let pagination = prms.Pager

        { Items = items
          Pager =
              { Total = int64 count
                Index = pagination.Index
                Size = pagination.Size } }


    let executeListQuery prms q =
        task {
            let! result = executeListQuery' q
            return result |> mapResponse prms
        }

    let executeGroupByQuery prms fn qcount q =
        task {
            let! result = executeGroupByQuery' fn qcount q
            return result |> mapResponse prms
        }
