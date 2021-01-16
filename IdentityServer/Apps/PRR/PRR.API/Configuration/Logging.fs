namespace PRR.API.Configuration

open System.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging

open Serilog
open Serilog.Configuration
open Serilog.Configuration

[<AutoOpen>]
module private Logging =

    let configureLogging (services: IServiceCollection) =
        printfn "configureLogging"
        services.AddLogging(fun (builder: ILoggingBuilder) -> builder.ClearProviders().AddSerilog() |> ignore)
        |> ignore

        Log.Logger <-
            LoggerConfiguration().Enrich.FromLogContext().WriteTo.Console().WriteTo.Seq("http://localhost:5341")
                .CreateLogger()
