namespace PRR.API.Common.Configuration

open System
open System.Collections.Generic
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Diagnostics.HealthChecks
open MassTransit

type HealthCheckConfig =
    { AllocatedMemoryGb: int
      PsqlConnectionString: string
      MongoConnectionString: string }

[<AutoOpen>]
module HealthCheck =

    let configureHealthCheck (config: HealthCheckConfig) (services: IServiceCollection) =
        services.AddHealthChecks().AddProcessAllocatedMemoryHealthCheck(int config.AllocatedMemoryGb * int (1E+9))
                .AddNpgSql(config.PsqlConnectionString,
                           null,
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
