namespace Common.Test.Utils

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

    let httpPostAsync (client: HttpClient) (bearer: string) (path: string) (payload: 'a) =
        client.DefaultRequestHeaders.Authorization <- Headers.AuthenticationHeaderValue("Bearer", bearer)
        httpPostAsync' client path payload

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
