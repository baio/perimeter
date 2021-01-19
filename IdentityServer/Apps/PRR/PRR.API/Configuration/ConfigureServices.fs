namespace PRR.API.Configuration

open Microsoft.Extensions.DependencyInjection

[<AutoOpen>]
module ConfigureServices =

    type Env =
        { Logging: LoggingEnv
          Tracing: TracingEnv }

    let configureServices' (env: Env) (services: IServiceCollection) =
        configureLogging env.Logging services
        configureTracing env.Tracing services
