namespace DataAvail.Giraffe.Common

[<AutoOpen>]
module WrapHandler =

    open System.Threading.Tasks
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Giraffe
    open Microsoft.AspNetCore.Http
    open DataAvail.Common

    let wrapHandler fn next ctx =
        ctx
        |> fn
        |> TaskUtils.bind (fun hr -> hr ctx next)

    let wrapHandlerOK (fn: HttpContext -> Task<'a>) next ctx =
        task {
            let! result = fn ctx
            return! Successful.OK result next ctx
        }


    let wrapHandlerNoContent (fn: HttpContext -> Task<unit>) next ctx =
        task {
            do! fn ctx
            return! Successful.NO_CONTENT next ctx
        }
