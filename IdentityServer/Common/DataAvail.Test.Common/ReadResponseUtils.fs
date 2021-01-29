namespace DataAvail.Test.Common

open Newtonsoft.Json

[<AutoOpen>]
module ReadResponseUtils =

    open FSharp.Control.Tasks.V2.ContextInsensitive
    open System.Net.Http
    open Utf8Json

    let readAsTextAsync (response: HttpResponseMessage) =
        response.Content.ReadAsStringAsync()

    let readAsJsonAsync<'a> (response: HttpResponseMessage) =
        task {            
            let! str = response.Content.ReadAsStringAsync()
            let json = JsonConvert.DeserializeObject<'a>(str)
            return json
        }

    let readResponseHader (name: string) (response: HttpResponseMessage) =                    
        response.Headers.GetValues(name) |> Seq.head
