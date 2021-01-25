﻿namespace PRR.API.Configuration

open System
open System.Collections.Generic
open Common.Domain.Models
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Diagnostics.HealthChecks

type HealthCheckConfig =
    { AllocatedMemory: int<gigabytes>
      PsqlConnectionString: string
      MongoConnectionString: string }

[<AutoOpen>]
module HealthCheck =

    let configureHealthCheck (config: HealthCheckConfig) (services: IServiceCollection) =
        services.AddHealthChecks().AddProcessAllocatedMemoryHealthCheck(int config.AllocatedMemory * int (1E+9))
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
        ()