module PRR.API.Tenant.App

open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open System
open PRR.API.Tenant.Configuration
open PRR.API.Common.ErrorHandler
open PRR.API.Tenant.Routes
open Prometheus

let webApp =
    subRoute
        "/api/tenant"
        (choose [ createRoutes ()
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

    configureAppServices appConfig services


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
            .UseUrls("http://*:6000", "https://*:6001")
            .UseIISIntegration()
            .ConfigureAppConfiguration(configureAppConfiguration)
            .Configure(Action<IApplicationBuilder> configureApp)
            .ConfigureServices(configureServices)
            .Build()

    app.Run()
    0
