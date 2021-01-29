namespace DataAvail.ListQuery.EntityFramework

open Microsoft.EntityFrameworkCore
open FSharp.Control.Tasks.V2.ContextInsensitive

[<AutoOpen>]
module private Utils =

    let toListAsync =
        EntityFrameworkQueryableExtensions.ToListAsync

    let countAsync =
        EntityFrameworkQueryableExtensions.CountAsync


    let groupByAsync''' (fn: System.Tuple<_, _> -> _) x =
        task {
            let! res = x |> toListAsync
            return res |> Seq.groupBy fn
        }

    let groupByAsync'' x = x |> groupByAsync''' fst

    let groupByAsync' x =
        task {
            let! grp = x |> groupByAsync''

            return grp
                   |> (Seq.map (fun (k, v) -> (k, v |> Seq.map snd)))
        }

    let groupByAsync fn x =
        task {
            let! grp = groupByAsync' x
            return Seq.map fn grp
        }
