namespace Common.Test.Utils

open Json.Net
open Newtonsoft.Json
open System.Collections.Generic

[<AutoOpen>]
module HttpMethods =

    open System.Net.Http
    open System.Text
    open Utf8Json


    let httpGetAsync' (client: HttpClient) (path: string) = path |> client.GetAsync

    let httpGetAsync (client: HttpClient) (bearer: string) (path: string) =
        client.DefaultRequestHeaders.Authorization <- Headers.AuthenticationHeaderValue("Bearer", bearer)
        httpGetAsync' client path

    let httpPostAsync' (client: HttpClient) (path: string) (payload: 'a) =
        let str = JsonSerializer.ToJsonString(payload)
        use json = new StringContent(str, Encoding.UTF8, "application/json")
        client.PostAsync(path, json)


    let httpPostFormAsync' (client: HttpClient) (path: string) (payload: Map<string, string>) =
        let pairs =
            payload
            |> Map.toSeq
            |> Seq.map (fun (k, v) -> KeyValuePair(k, v))

        let formContent = new FormUrlEncodedContent(pairs)
        client.PostAsync(path, formContent)

    let httpPostAsync (client: HttpClient) (bearer: string) (path: string) (payload: 'a) =
        client.DefaultRequestHeaders.Authorization <- Headers.AuthenticationHeaderValue("Bearer", bearer)
        httpPostAsync' client path payload

    let httpPostFormAsync (client: HttpClient) (bearer: string) (path: string) (payload) =
        client.DefaultRequestHeaders.Authorization <- Headers.AuthenticationHeaderValue("Bearer", bearer)
        httpPostFormAsync' client path payload

    let httpPostFormJsonAsync (client: HttpClient) (bearer: string) (path: string) (payload) =
        let str = JsonSerializer.ToJsonString(payload)
        let json = JsonConvert.DeserializeObject<Map<string, string>>(str)
        httpPostFormAsync client bearer path json
        
    let httpPutAsync' (client: HttpClient) (path: string) (payload: 'a) =
        let str = JsonSerializer.ToJsonString(payload)
        use json = new StringContent(str, Encoding.UTF8, "application/json")
        client.PutAsync(path, json)

    let httpPutAsync (client: HttpClient) (bearer: string) (path: string) (payload: 'a) =
        client.DefaultRequestHeaders.Authorization <- Headers.AuthenticationHeaderValue("Bearer", bearer)
        httpPutAsync' client path payload

    let httpPatchAsync' (client: HttpClient) (path: string) (payload: 'a) =
        let str = JsonSerializer.ToJsonString(payload)
        use json = new StringContent(str, Encoding.UTF8, "application/json")
        client.PatchAsync(path, json)

    let httpPatchAsync (client: HttpClient) (bearer: string) (path: string) (payload: 'a) =
        client.DefaultRequestHeaders.Authorization <- Headers.AuthenticationHeaderValue("Bearer", bearer)
        httpPatchAsync' client path payload

    let httpDeleteAsync' (client: HttpClient) (path: string) =
        client.DeleteAsync(path)

    let httpDeleteAsync (client: HttpClient) (bearer: string) (path: string) =
        client.DefaultRequestHeaders.Authorization <- Headers.AuthenticationHeaderValue("Bearer", bearer)
        httpDeleteAsync' client path
