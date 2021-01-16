namespace PRR.API.Configuration

open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging

open Serilog
open Serilog.Events

[<AutoOpen>]
module private Logging =

    let private filterIgnoredEndpoints =
        System.Func<_, _>(fun (logEvent: LogEvent) ->
            let (f, path) =
                logEvent.Properties.TryGetValue("RequestPath")

            match f with
            | true ->
                let path' = path.ToString().Trim('"')
                path' = "/metrics"
            | false -> false)

    let configureLogging (services: IServiceCollection) =
        printfn "configureLogging"
        services.AddLogging(fun (builder: ILoggingBuilder) ->
            builder.ClearProviders().AddFilter("Microsoft", LogLevel.None).AddSerilog()
            |> ignore)
        |> ignore

        Log.Logger <-
            LoggerConfiguration().Enrich.FromLogContext().WriteTo.Console().WriteTo.Seq("http://localhost:5341")
                .Filter.ByExcluding(filterIgnoredEndpoints).CreateLogger()
