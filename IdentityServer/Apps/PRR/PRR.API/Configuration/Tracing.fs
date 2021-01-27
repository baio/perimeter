namespace PRR.API.Configuration

open System.Diagnostics
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open OpenTelemetry.Trace
open OpenTelemetry.Contrib.Instrumentation.MassTransit

type TracingConfig =
    { ServiceName: string
      AgentHost: string
      AgentPort: int }

type TracingEnv =
    { Config: TracingConfig
      IgnoreApiPaths: string seq }

[<AutoOpen>]
module private Tracing =

    let private filterIgnoredEndpoints ignorePaths =
        System.Func<_, _>(fun (context: HttpContext) ->
            let path = context.Request.Path.Value
            Seq.contains path ignorePaths |> not)


    let configureTracing (env: TracingEnv) (services: IServiceCollection) =

#if !TEST
        // Change default activity format to OpenTelemetry
        Activity.DefaultIdFormat <- ActivityIdFormat.W3C
        Activity.ForceDefaultIdFormat <- true

        services.AddOpenTelemetryTracing(fun (builder: TracerProviderBuilder) ->
            builder.AddAspNetCoreInstrumentation(fun ops -> ops.Filter <- filterIgnoredEndpoints env.IgnoreApiPaths)
                   .AddJaegerExporter(fun c ->
                   c.ServiceName <- env.Config.ServiceName // "api"
                   c.AgentHost <- env.Config.AgentHost // "localhost"
                   c.AgentPort <- env.Config.AgentPort) // 6831)
                   .AddEntityFrameworkCoreInstrumentation(fun ops -> ops.SetTextCommandContent <- true)
                   .AddMassTransitInstrumentation()
            |> ignore)
        |> ignore
#endif
        ()
