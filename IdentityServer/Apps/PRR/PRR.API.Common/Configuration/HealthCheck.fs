namespace PRR.API.Common.Configuration

open System
open System.Collections.Generic
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Diagnostics.HealthChecks
open MassTransit
open RabbitMQ.Client
// https://andrewlock.net/deploying-asp-net-core-applications-to-kubernetes-part-6-adding-health-checks-with-liveness-readiness-and-startup-probes/
type HealthCheckConfig =
    { AllocatedMemoryGb: int
      PsqlConnectionString: string
      MongoConnectionString: string
      RabbitMqConnectionString: string }

[<AutoOpen>]
module HealthCheck =

    let configureHealthCheck (config: HealthCheckConfig) (services: IServiceCollection) =

        // https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks/issues/552
        services.AddSingleton<IConnection>(fun sp ->
            let amqpUrl =
                sprintf "%s%s"
                config.RabbitMqConnectionString
                if config.RabbitMqConnectionString.StartsWith("amqp://") then "" else "amqp://"
                
            let factory =
                ConnectionFactory
                    (Uri = Uri(amqpUrl), AutomaticRecoveryEnabled = true)

            factory.CreateConnection())

        services
            .AddHealthChecks()
            .AddProcessAllocatedMemoryHealthCheck(int config.AllocatedMemoryGb * int (1E+9))
            .AddRabbitMQ(config.RabbitMqConnectionString,
                         Nullable<HealthStatus>(HealthStatus.Degraded),
                         null,
                         (Nullable<TimeSpan>(TimeSpan(int64 (1E+6)))))
            .AddNpgSql(config.PsqlConnectionString,
                       "SELECT 1;",
                       null,
                       null,
                       Nullable<HealthStatus>(HealthStatus.Degraded),
                       null,
                       (Nullable<TimeSpan>(TimeSpan(int64 (1E+6)))))
            .AddMongoDb(config.MongoConnectionString,
                        null,
                        Nullable<HealthStatus>(HealthStatus.Degraded),
                        null,
                        (Nullable<TimeSpan>(TimeSpan(int64 (1E+6)))))
        |> ignore


        services.Configure(fun (options: HealthCheckPublisherOptions) ->
            options.Delay <- TimeSpan.FromSeconds(2.)
            options.Predicate <- (fun check -> check.Tags.Contains("ready")))
        |> ignore
