﻿namespace PRR.API.Common

open Giraffe
open Microsoft.Extensions.Logging
open System
open DataAvail.EntityFramework.Common
open DataAvail.Http.Exceptions

module ErrorHandler =

    [<CLIMutable>]
    type ErrorDTO =
        { Message: string
          Field: string
          Code: string }

    type ErrorDataDTO<'a> = { Message: string; Data: 'a }

    let mapBadRequestError =
        function
        | BadRequestFieldError (field, err) ->
            let errMessage =
                match err with
                | CUSTOM msg -> (sprintf "CUSTOM:%s" msg)
                | _ ->
                    (sprintf "%O" err)
                        .Replace(" ", ":")
                        .Replace("\"", "")

            (field, errMessage)
        | BadRequestCommonError x -> ("__", x)

    let mapBadRequestErrors x =
        x
        |> Array.map mapBadRequestError
        |> Array.groupBy fst
        |> Array.map (fun (key, vals) -> (key, (vals |> Seq.map snd)))
        |> Map.ofArray

    let rec findLeafInnerException (ex: Exception) =
        if (ex.InnerException = null)
        then ex
        else findLeafInnerException ex.InnerException

    let matchException (logger: ILogger) (ex: Exception) =
        logger.LogWarning("Handle exception {@exception}", ex)

        match ex with
        | :? Unexpected as e ->
            let msg =
                match e.Data0 with
                | Some x -> x
                | None -> "Unexpected error"

            ServerErrors.INTERNAL_ERROR
                { Message = msg
                  Field = null
                  Code = null }
        | :? NotFound ->
            RequestErrors.NOT_FOUND
                { Message = "Not Found"
                  Field = null
                  Code = null }
        | :? UnAuthorized as e ->
            let msg =
                match e.Data0 with
                | Some x -> x
                | None -> "Not Authorized"

            RequestErrors.UNAUTHORIZED
                "Bearer"
                "App"
                { Message = msg
                  Field = null
                  Code = null }
        | :? Forbidden as e ->
            let msg =
                match e.Data0 with
                | Some x -> x
                | None -> "Forbidden"

            RequestErrors.FORBIDDEN
                { Message = msg
                  Field = null
                  Code = null }
        | :? Conflict as e ->
            match e.Data0 with
            | ConflictErrorField (field, code) ->
                RequestErrors.CONFLICT
                    { Message = null
                      Field = field
                      Code = (sprintf "%O" code) }
            | ConflictErrorCommon msg ->
                RequestErrors.CONFLICT
                    { Message = msg
                      Field = null
                      Code = null }
        | :? BadRequest as e ->
            e.Data0
            |> mapBadRequestErrors
            |> fun errs ->
                { Message = "Some data is not valid"
                  Data = errs }
            |> RequestErrors.BAD_REQUEST
        | _ -> ServerErrors.INTERNAL_ERROR ex

    let mapException logger =
        findLeafInnerException >> matchException logger

    let errorHandler (ex: Exception) (logger: ILogger) =
        clearResponse >=> (mapException logger ex)
