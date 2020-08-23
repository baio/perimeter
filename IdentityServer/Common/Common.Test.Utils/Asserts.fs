namespace Common.Test.Utils

[<AutoOpen>]
module Asserts =

    open FSharp.Control.Tasks.V2
    open System.Net

    open System.Net.Http

    let ensureSuccessAsync (response: HttpResponseMessage) =
        task {
            if not response.IsSuccessStatusCode then
                printf "Statsu Code: %s" (response.StatusCode.ToString())
                let! result = response.Content.ReadAsStringAsync()
                result |> failwithf "%A"
        }

    let ensureFail (code: HttpStatusCode) (response: HttpResponseMessage) =
        if response.IsSuccessStatusCode || response.StatusCode <> code then
            printf "Statsu Code: %s" (response.StatusCode.ToString())
            failwithf "%A" response

    let ensureUnauthorized x = x |> ensureFail HttpStatusCode.Unauthorized

    let ensureForbidden x = x |> ensureFail HttpStatusCode.Forbidden

    let ensureConflict x = x |> ensureFail HttpStatusCode.Conflict

    let ensureBadRequest x = x |> ensureFail HttpStatusCode.BadRequest

    let ensureRedirectErrorAsync (response: HttpResponseMessage) =
        task {
            if response.StatusCode <> HttpStatusCode.Found then
                printf "Status Code: %s" (response.StatusCode.ToString())
                let! result = response.Content.ReadAsStringAsync()
                result |> failwithf "%s"
            else
                match response.Headers.GetValues("Location") |> Seq.tryHead with
                | Some x ->
                    if not (x.Contains("error")) then failwithf "Not contains error"
                | _ -> failwithf "Not contains location"
        }

    let ensureRedirectSuccessAsync (response: HttpResponseMessage) =
        task {
            if response.StatusCode <> HttpStatusCode.Found then
                printf "Status Code: %s" (response.StatusCode.ToString())
                let! result = response.Content.ReadAsStringAsync()
                result |> failwithf "%s"
            else
                match response.Headers.GetValues("Location") |> Seq.tryHead with
                | Some x ->
                    if x.Contains("error") then failwithf "Not contains error"
                | _ -> failwithf "Not contains location"
        }




    let ensureNotFound x = x |> ensureFail HttpStatusCode.NotFound
