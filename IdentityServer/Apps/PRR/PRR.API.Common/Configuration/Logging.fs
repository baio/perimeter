namespace PRR.API.Common.Configuration

open System
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging

open Serilog
open Serilog.Events
open Serilog.Sinks.Elasticsearch

type LoggingConfig =
    { SeqServiceUrl: string
      ElasticServiceUrl: string }

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

        services.AddLogging(fun (builder: ILoggingBuilder) ->
            builder
                .ClearProviders()
                .AddSerilog()
                .SetMinimumLevel(LogLevel.Debug)
            |> ignore)
        |> ignore

        Log.Logger <-
            let mutable config =
                LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                    // Filter out ASP.NET Core infrastructre logs that are Information and below
                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
            #if !TEST
            let useSeq =
                not (String.IsNullOrEmpty env.Config.SeqServiceUrl)

            let useElastic =
                not (String.IsNullOrEmpty env.Config.ElasticServiceUrl)

            if useSeq
            then config <- config.WriteTo.Seq env.Config.SeqServiceUrl

            if useElastic
            then config <- config.WriteTo.Elasticsearch(ElasticsearchSinkOptions(Uri env.Config.ElasticServiceUrl))

            if (not useElastic) && (not useSeq) then config <- config.WriteTo.Console()
            #else

            #endif
            config
                .Filter
                .ByExcluding(filterIgnoredEndpoints env.IgnoreApiPaths)
                .CreateLogger()
