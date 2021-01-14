namespace Common.Domain.Giraffe

open Common.Domain.Models
open Common.Domain.Models.Exceptions
open Common.Utils
open System.Collections.Generic
open System.ComponentModel.DataAnnotations
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

    let private validateDataAnnotations (object: obj) =
        let ctx = ValidationContext object
        let errors = List<ValidationResult>()
        if Validator.TryValidateObject(object, ctx, errors, true) then None
        else Some errors

    let bindValidateAnnotatedJsonAsync<'a> (ctx: HttpContext) =
        task {
            let! model = bindJsonAsync<'a> ctx
            let errs = validateDataAnnotations model
            match errs with
            | None -> return model
            | Some errors ->
                let err =
                    errors
                    |> Seq.map (fun x ->
                        let field = x.MemberNames |> Seq.head
                        BadRequestFieldError(field, CUSTOM x.ErrorMessage))
                    |> Seq.toArray
                    |> BadRequest

                return raise err
        }

    let bindValidateFormAsync<'a> validator (ctx: HttpContext) =
        task {
            try
                let! model = ctx.BindFormAsync<'a>()
                match validator model with
                | [||] -> return model
                | errors ->
                    printfn "bindValidateFormAsync: Validation errors %O" errors
                    return raise (BadRequest errors)
            with ex ->
                return raise (BadRequest [| (BadRequestCommonError ex.Message) |])
        }


    let bindFormAsync<'a> (ctx: HttpContext) =
        ctx.BindFormAsync<'a>()
