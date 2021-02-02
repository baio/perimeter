namespace PRR.API.Common.Configuration

open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open PRR.Data.DataContext
open Serilog

type DataContextConfig =
    { ConnectionString: string
      MigrationAssembly: string option }

[<AutoOpen>]
module DataContext =

    let configureDataContext (config: DataContextConfig) (services: IServiceCollection) =

        let loggerFactory =
            LoggerFactory.Create(fun builder -> builder.AddSerilog() |> ignore)

        let connectionString = config.ConnectionString

        services.AddDbContext<DbDataContext>
            ((fun o ->
                let o' =
                    o
                        .UseLoggerFactory(loggerFactory).EnableSensitiveDataLogging true

                NpgsqlDbContextOptionsExtensions.UseNpgsql
                    (o',
                     connectionString,
                     (fun b ->
                         let b = b.EnableRetryOnFailure(1)
                         match config.MigrationAssembly with
                         | Some migrationAssembly -> b.MigrationsAssembly(migrationAssembly) |> ignore
                         | None -> ()))
                |> ignore))
        |> ignore
