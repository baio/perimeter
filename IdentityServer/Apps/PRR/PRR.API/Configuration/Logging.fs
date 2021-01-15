namespace PRR.API.Configuration

open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging

open Serilog

[<AutoOpen>]
module private Logging =

    let configureLogging (services: IServiceCollection) =
        printfn "configureLogging"
        services.AddLogging(fun (builder: ILoggingBuilder) -> builder.ClearProviders().AddSerilog() |> ignore)
        |> ignore

        Log.Logger <- LoggerConfiguration().Enrich.FromLogContext().WriteTo.Console().CreateLogger()
