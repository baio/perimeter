namespace PRR.API.Common.Configuration

open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging

open Serilog
open Serilog.Events

type LoggingConfig = { ServiceUrl: string }

type LoggingEnv =
    { Config: LoggingConfig
      IgnoreApiPaths: string seq }

[<AutoOpen>]
module Logging =

    let private filterIgnoredEndpoints ignorePaths =
        System.Func<_, _>(fun (logEvent: LogEvent) ->
            let (f, path) =
                logEvent.Properties.TryGetValue("RequestPath")

            match f with
            | true ->
                let path' = path.ToString().Trim('"')
                Seq.contains path' ignorePaths
            | false -> false)

    let configureLogging (env: LoggingEnv) (services: IServiceCollection) =

        services.AddLogging(fun (builder: ILoggingBuilder) -> builder.ClearProviders().AddSerilog() |> ignore)
        |> ignore

        if env.Config.ServiceUrl <> null then
            Log.Logger <-
                LoggerConfiguration().Enrich.FromLogContext()
                    #if !TEST 
                    .WriteTo.Seq(env.Config.ServiceUrl)
                    #else
                    .WriteTo.Console().WriteTo.Debug()
                    #endif
                    .Filter.ByExcluding(filterIgnoredEndpoints env.IgnoreApiPaths).CreateLogger()
