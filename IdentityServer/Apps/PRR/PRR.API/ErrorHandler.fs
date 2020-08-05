namespace PRR.API

open Common.Domain.Models
open Giraffe
open Microsoft.Extensions.Logging
open System

[<AutoOpen>]
module ErrorHandler =

    type ErrorDTO<'a> =
        { Message: string
          Data: 'a }

    let mapBadRequestError =
        function
        | BadRequestFieldError(field, err) ->
            (field, (sprintf "%O" err).Replace(" ", ":").Replace("\"", ""))
        | BadRequestCommonError x -> ("__", x)

    let mapBadRequestErrors x =
        x
        |> Array.map mapBadRequestError
        |> Array.groupBy fst
        |> Array.map (fun (key, vals) -> (key, (vals |> Seq.map snd)))
        |> Map.ofArray


    let rec findLeafInnerException (ex: Exception) =
        if (ex.InnerException = null) then ex
        else findLeafInnerException ex.InnerException

    let matchException (ex: Exception) =
        printf "Error : %O" ex
        match ex with
        | :? NotFound ->
            RequestErrors.NOT_FOUND "Not Found"
        | :? UnAuthorized as e ->
            let msg = match e.Data0 with Some x -> x | None -> "Not Authorized"
            RequestErrors.UNAUTHORIZED "Bearer" "App" msg
        | :? Forbidden ->
            RequestErrors.FORBIDDEN "Forbidden"
        | :? Conflict as e ->
            RequestErrors.CONFLICT e.Data0
        | :? BadRequest as e ->
            e.Data0
            |> mapBadRequestErrors
            |> fun errs ->
                { Message = "Some data is not valid"
                  Data = errs }
            |> RequestErrors.BAD_REQUEST
        | _ ->
            ServerErrors.INTERNAL_ERROR ex

    let mapException (ex: Exception) =
        match ex with
        | :? AggregateException ->
            ex
            |> findLeafInnerException
            |> matchException
        | _ ->
            matchException ex

    let errorHandler (ex: Exception) (logger: ILogger) =
        clearResponse >=> (mapException ex)
