namespace PRR.API.Configuration

open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open OpenTelemetry.Trace

[<AutoOpen>]
module private Tracing =

    let private filterIgnoredEndpoints =
        System.Func<_, _>(fun (context: HttpContext) ->
            let path = context.Request.Path.Value
            path <> "/metrics")


    let configureTracing (services: IServiceCollection) =
        services.AddOpenTelemetryTracing(fun (builder: TracerProviderBuilder) ->
            builder.AddAspNetCoreInstrumentation(fun ops -> ops.Filter <- filterIgnoredEndpoints)
                   .AddJaegerExporter(fun c ->
                   c.ServiceName <- "api"
                   c.AgentHost <- "localhost"
                   c.AgentPort <- 6831)
                   .AddEntityFrameworkCoreInstrumentation(fun ops -> ops.SetTextCommandContent <- true)
            |> ignore)
        |> ignore
