namespace Common.Test.Utils

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

            let! stream = response.Content.ReadAsStringAsync()
            let json = JsonConvert.DeserializeObject<'a>(stream) (*
                            JsonSerializer.DeserializeAsync<'a>
                            (stream, Utf8Json.Resolvers.StandardResolver.CamelCase) //; JsonSerializer.DeserializeAsync<'a>(stream)
            *)
            return json
        }
