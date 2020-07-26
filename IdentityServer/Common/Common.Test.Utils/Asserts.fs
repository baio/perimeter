namespace Common.Test.Utils

[<AutoOpen>]
module Asserts =

    open System.Net
    open System.Net.Http

    open FSharp.Control.Tasks.V2

    let ensureSuccessAsync (response: HttpResponseMessage) =
        task {
            if not response.IsSuccessStatusCode then
                printf "Statsu Code: %s" (response.StatusCode.ToString())
                let! result = response.Content.ReadAsStringAsync()
                result |> failwithf "%A"
        }

    let ensureFail (code: HttpStatusCode)  (response: HttpResponseMessage) =
        if response.IsSuccessStatusCode || response.StatusCode <> code then
            printf "Statsu Code: %s" (response.StatusCode.ToString())
            failwithf "%A" response                       

    let ensureUnauthorized x = x |> ensureFail HttpStatusCode.Unauthorized

    let ensureForbidden x = x |> ensureFail HttpStatusCode.Forbidden

    let ensureConflict x = x |> ensureFail HttpStatusCode.Conflict
    
    let ensureBadRequest x = x |> ensureFail HttpStatusCode.BadRequest
    
    let ensureNotFound x = x |> ensureFail HttpStatusCode.NotFound


