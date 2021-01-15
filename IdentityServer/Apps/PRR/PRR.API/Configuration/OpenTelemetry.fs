namespace PRR.API.Configuration

open Microsoft.Extensions.DependencyInjection
open OpenTelemetry.Trace

[<AutoOpen>]
module private OpenTelemetry =

    let configureOpenTelemetry (services: IServiceCollection) =
        services.AddOpenTelemetryTracing(fun builder ->
            builder.AddAspNetCoreInstrumentation()
                   .AddJaegerExporter(fun c ->
                   c.AgentHost <- "localhost"
                   c.AgentPort <- 6831)
            |> ignore)
        |> ignore
