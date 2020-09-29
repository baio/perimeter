namespace FSharp.Mongo.ListQuery

open Common.Domain.Models
open FSharp.Control.Tasks.V2.ContextInsensitive
open FSharp.MongoDB.Driver

[<AutoOpen>]
module ExecuteListQuery =

    let executeListQuery' q =
       
        task {
            // can't run in parallel https://go.microsoft.com/fwlink/?linkid=2097913.))))"
            let! list = q |> toListAsync
            let! count = q |> countAsync
            return (list, count)
        }

    let mapResponse (prms: ListQuery<_, _>) (items: 'a seq, count: int64) =
        let pagination = prms.Pager
        
        { 
            Items = items
            Pager = { 
                Total = count
                Index = pagination.Index
                Size = pagination.Size 
            } 
        }
                

    let executeListQuery prms q =
        task {
            let! result = executeListQuery' q
            return result |> mapResponse prms
        }

