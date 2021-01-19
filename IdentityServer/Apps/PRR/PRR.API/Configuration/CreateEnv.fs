namespace PRR.API.Configuration

open Microsoft.Extensions.Configuration

[<AutoOpen>]
module CreateEnv =

    let createEnv (configuration: IConfiguration) =

        let ignoreObserveApiPaths = [ "/metrics" ]

        { Logging =
              { Config = { ServiceUrl = configuration.GetValue("Logging:Seq:ServiceUrl") }
                IgnoreApiPaths = ignoreObserveApiPaths }
          Tracing =
              { Config =
                    { ServiceName = configuration.GetValue("Tracing:Jaeger:ServiceName")
                      AgentHost = configuration.GetValue("Tracing:Jaeger:AgentHost")
                      AgentPort = configuration.GetValue<int>("Tracing:Jaeger:AgentPort") }
                IgnoreApiPaths = ignoreObserveApiPaths } }
