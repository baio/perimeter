namespace Common.Domain.Giraffe

open System.Threading.Tasks
open FSharp.Control.Tasks.V2.ContextInsensitive
open Common.Utils
open Giraffe
open Microsoft.AspNetCore.Http

[<AutoOpen>]
module WrapHandler =
    let wrapHandler fn next ctx =
        ctx
        |> fn
        |> TaskUtils.bind (fun hr -> hr ctx next)


    let wrapHandlerNoContent (fn: HttpContext -> Task<unit>) next ctx =
        task {
            do! fn ctx
            return! Successful.NO_CONTENT next ctx
        }
