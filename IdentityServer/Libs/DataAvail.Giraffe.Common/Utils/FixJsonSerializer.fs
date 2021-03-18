namespace DataAvail.Giraffe.Common.Utils

open Newtonsoft.Json.Serialization

[<RequireQualifiedAccess>]
module FixJsonSerializer =

    open Giraffe.Serialization
    open Microsoft.Extensions.DependencyInjection
    open Microsoft.FSharpLu.Json
    open Newtonsoft.Json

    let configureServices (services: IServiceCollection) =

        let contractResolver = CamelCasePropertyNamesContractResolver()
        
        let customSettings =
            JsonSerializerSettings(ContractResolver = contractResolver)

        customSettings.Converters.Add(CompactUnionJsonConverter(true))
        
        let serializer = NewtonsoftJsonSerializer(customSettings)
        
        services.AddSingleton<IJsonSerializer>(serializer)
        |> ignore
