namespace DataAvail.Giraffe.Common

[<AutoOpen>]
module Wrap =

    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Giraffe
    open Microsoft.AspNetCore.Http
    open System.Threading.Tasks

    type HandlerFun<'s> = HttpContext -> Task<Task<'s>>

    let wrap (handler: HandlerFun<_>) (next: HttpFunc) (ctx: HttpContext) =
        task {
            let! hr = handler ctx
            let! result = hr
            return! json result next ctx }

    type TaskFun<'a> = HttpContext -> Task<'a>

    let wrapTask<'a> (handler: TaskFun<'a>) (fn: 'a -> HttpFunc -> HttpFunc) (next: HttpFunc) (ctx: HttpContext) =
        task {
            let! result = handler ctx
            return! fn result next ctx }
