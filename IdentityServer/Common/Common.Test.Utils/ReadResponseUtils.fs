namespace Common.Test.Utils

[<AutoOpen>]
module ReadResponseUtils = 

    open FSharp.Control.Tasks.V2.ContextInsensitive    
    open System.Net.Http    
    open Utf8Json

    let readAsTextAsync (response: HttpResponseMessage) =
        response.Content.ReadAsStringAsync() 

    let readAsJsonAsync<'a> (response: HttpResponseMessage) =
        task {
            let! stream = response.Content.ReadAsStreamAsync()
            let! json = JsonSerializer.DeserializeAsync<'a>(stream)
            return json
        }


