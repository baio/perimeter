namespace Common.Domain.Giraffe

open Common.Domain.Models.Exceptions
open Common.Domain.Models
open Common.Utils
open System.Globalization
open System.IO

[<AutoOpen>]
module Payload =

    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Giraffe
    open Microsoft.AspNetCore.Http

    type private PropertyBag = BoundModelValue of System.Type

    let bindJsonAsync<'a> (ctx: HttpContext) =
        // As soon as model bound first time it erased from context, so we should persist one if we want to bind it many times        
        task {
            let t = typeof<'a>
            let (f, v) = ctx.Items.TryGetValue(BoundModelValue t)
            if f then
                return v :?> 'a
            else
                let! v = ctx.BindModelAsync<'a>()
                ctx.Items.Add(BoundModelValue typeof<'a>, v)
                return v
        }

    let bindValidateJsonAsync<'a> validator (ctx: HttpContext) =
        task {
            let! model = bindJsonAsync<'a> ctx
            match validator model with
            | [||] -> return model
            | errors ->
                return raise (BadRequest errors)
        }

    let bindValidateFormAsync<'a> validator (ctx: HttpContext) =
        task {
            try
                let! model = ctx.BindFormAsync<'a>()
                match validator model with
                | [||] -> return model
                | errors ->
                    return raise (BadRequest errors)
            with  
            | ex ->
                printfn "--- %O" ex
                return raise (BadRequest [| (BadRequestCommonError ex.Message) |])
        }
