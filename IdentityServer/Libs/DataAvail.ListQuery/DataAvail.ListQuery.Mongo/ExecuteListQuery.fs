namespace DataAvail.ListQuery.Core

open FSharp.Control.Tasks.V2.ContextInsensitive
open MongoDB.Driver

[<AutoOpen>]
module ExecuteListQuery =


    let executeListQuery' (q: IFindFluent<_, _>) =

        task {
            let! list = q.ToListAsync()
            let! count = q.CountDocumentsAsync()
            return (list, count)
        }


    let mapResponse (prms: ListQuery<_, _>) (items: 'a seq, count: int64) =
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
