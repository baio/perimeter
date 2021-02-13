namespace PRR.API.Common.Configuration

open Microsoft.Extensions.Configuration

type CommonAppConfig =
    { Logging: LoggingEnv
      Tracing: TracingEnv
      DataContext: DataContextConfig
      HealthCheck: HealthCheckConfig
      ViewStorage: ViewStorageConfig
      ServiceBus: ServiceBus.Config }

[<AutoOpen>]
module CreateCommonAppConfig =

    let createCommonAppConfig (configuration: IConfiguration) =

        let viewStorageConfig: ViewStorageConfig =
            { ConnectionString = configuration.GetValue("MongoViewStorage:ConnectionString")
              DbName = configuration.GetValue("MongoViewStorage:DbName") }

        let ignoreObserveApiPaths = [ "/metrics"; "/health" ]

        let psqlConnectionString =
            configuration.GetConnectionString "PostgreSQL"

        let mongoConnectionString =
            configuration.GetConnectionString "Mongo"

        { ServiceBus = { Host = configuration.GetValue("ServiceBus:Host") }
          ViewStorage = viewStorageConfig
          HealthCheck =
              { PsqlConnectionString = psqlConnectionString
                MongoConnectionString = mongoConnectionString
                AllocatedMemoryGb = 5 }
          DataContext =
              { ConnectionString = psqlConnectionString
                MigrationAssembly = Some "PRR.Data.DataContextMigrations" }
          Logging =
              { Config = { ServiceUrl = configuration.GetValue("Logging:Seq:ServiceUrl") }
                IgnoreApiPaths = ignoreObserveApiPaths }
          Tracing =
              { Config =
                    { ServiceName = configuration.GetValue("Tracing:Jaeger:ServiceName")
                      AgentHost = configuration.GetValue("Tracing:Jaeger:AgentHost")
                      AgentPort = configuration.GetValue<int option>("Tracing:Jaeger:AgentPort") }
                IgnoreApiPaths = ignoreObserveApiPaths } }
