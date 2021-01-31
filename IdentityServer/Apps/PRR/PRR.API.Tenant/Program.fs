﻿module PRR.API.App

open System.Net
open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Diagnostics.HealthChecks
open Microsoft.Extensions.Logging
open PRR.API
open PRR.API.Routes
open PRR.Data.DataContext
open System
open PRR.API.Tenant.Configuration
open Prometheus
open HealthChecks.Prometheus
open PRR.API.Common.ErrorHandler

let webApp =
    subRoute
        "/api"
        (choose [ Routes.Tenant.CreateRoutes.createRoutes ()
                  setStatusCode 404 >=> text "Not Found" ])

let configureCors (builder: CorsPolicyBuilder) =
    builder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
    |> ignore

let configureApp (app: IApplicationBuilder) =
    app
        .UseGiraffeErrorHandler(errorHandler)
        .UseAuthentication()
        .UseAuthorization()
        .UseCors(configureCors)
        // Configure metrics from prometheus
#if !TEST
        .UseHealthChecks(PathString("/health"))
        .UseHealthChecksPrometheusExporter(PathString("/metrics"))
        .UseMetricServer()
        .UseHttpMetrics()
#endif
        .UseGiraffe(webApp)

let configureServices (context: WebHostBuilderContext) (services: IServiceCollection) =

    services.AddCors().AddGiraffe() |> ignore

    let appConfig = createAppConfig context.Configuration

    configureAppServices appConfig services |> ignore

let configureAppConfiguration (context: WebHostBuilderContext) (config: IConfigurationBuilder) =

    let envName =
        (context.HostingEnvironment.EnvironmentName.ToLower())

    config
        .AddJsonFile("appsettings.json", false, true)
        .AddJsonFile(sprintf "appsettings.%s.json" envName, true)
        .AddEnvironmentVariables()
    |> ignore

[<EntryPoint>]
let main args =

    let config =
        ConfigurationBuilder()
            .AddCommandLine(args)
            .Build()

    let app =
        WebHostBuilder()
            .UseConfiguration(config)
            .UseKestrel()
            .UseUrls("http://*:5000", "https://*:5001")
            .UseIISIntegration()
            .ConfigureAppConfiguration(configureAppConfiguration)
            .Configure(Action<IApplicationBuilder> configureApp)
            .ConfigureServices(configureServices)
            .Build()

    app.Run()
    0
