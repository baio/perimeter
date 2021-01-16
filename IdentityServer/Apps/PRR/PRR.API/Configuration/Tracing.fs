namespace PRR.API.Configuration

open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open OpenTelemetry.Trace

[<AutoOpen>]
module private Tracing =

    let configureTracing (services: IServiceCollection) =
        services.AddOpenTelemetryTracing(fun (builder: TracerProviderBuilder) ->
            builder.AddAspNetCoreInstrumentation()
                   .AddJaegerExporter(fun c ->
                   c.ServiceName <- "api"
                   c.AgentHost <- "localhost"
                   c.AgentPort <- 6831)
                   .AddEntityFrameworkCoreInstrumentation(fun ops ->
                       ops.SetTextCommandContent <- true)
            |> ignore)
        |> ignore              
