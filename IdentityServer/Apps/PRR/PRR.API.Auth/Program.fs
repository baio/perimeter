module PRR.API.Auth.App

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
open PRR.API.Auth.Routes
open PRR.Data.DataContext
open System
open PRR.API.Auth.Configuration
open Prometheus
open HealthChecks.Prometheus

let webApp =
    subRoute
        "/api/auth"
        (choose [ createRoutes ()
#if E2E
                  E2E.createRoutes ()
#endif
                  setStatusCode 404 >=> text "Not Found" ])


let migrateDatabase (webHost: IWebHost) =
    // https://docs.microsoft.com/en-us/archive/msdn-magazine/2019/april/data-points-ef-core-in-a-docker-containerized-app#migrating-the-database
    use scope = webHost.Services.CreateScope()
    let services = scope.ServiceProvider

    try
        let db =
            services.GetRequiredService<DbDataContext>()

        db.Database.Migrate()
    with ex ->
        let logger =
            webHost.Services.GetRequiredService<ILogger<_>>()

        logger.LogCritical("An error occurred while migrating the database. {ex}", ex)


let configureCors (builder: CorsPolicyBuilder) =
    builder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
    |> ignore

let configureApp (app: IApplicationBuilder) =
    app
        .UseGiraffeErrorHandler(PRR.API.Common.ErrorHandler.errorHandler)
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

    let envName =
        (context.HostingEnvironment.EnvironmentName.ToLower())

    let env =
        createAppConfig envName context.Configuration

    configureAppServices env services

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

#if !TEST
    // test will apply migrations by itself
    migrateDatabase app
#endif
    app.Run()
    0