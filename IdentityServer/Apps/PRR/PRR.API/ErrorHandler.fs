namespace PRR.API

open System
open Microsoft.Extensions.Logging
open Common.Domain.Models
open Giraffe

[<AutoOpen>]
module ErrorHandler = 

    let rec findLeafInnerException (ex: Exception) =
        if (ex.InnerException = null) then
            ex
        else
            findLeafInnerException ex.InnerException

    let matchException (ex: Exception) =
            printf "Error : %O" ex
            match ex with 
            | :? NotFound -> 
                RequestErrors.NOT_FOUND "Not Found"
            | :? UnAuthorized -> 
                RequestErrors.UNAUTHORIZED "Bearer" "App" "Not Authorized"
            | :? Forbidden -> 
                RequestErrors.FORBIDDEN "Forbidden"
            | :? Conflict as e -> 
                RequestErrors.CONFLICT e.Data0
            | :? BadRequest as e ->
                RequestErrors.BAD_REQUEST e.Data0            
            | _ -> 
                ServerErrors.INTERNAL_ERROR  ex

    let mapException (ex: Exception) =
        match ex with 
        | :? AggregateException -> 
            ex |> findLeafInnerException |> matchException
        | _ -> 
            matchException ex

    let errorHandler (ex : Exception) (logger : ILogger) =
        clearResponse >=> (mapException ex)