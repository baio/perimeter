namespace PRR.API.Configuration

open Microsoft.EntityFrameworkCore
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open PRR.Data.DataContext
open Serilog

type DataContextConfig = { ConnectionString: string }

[<AutoOpen>]
module private ConfigureDataContext =

    let configureDataContext (config: DataContextConfig) (services: IServiceCollection) =
        let loggerFactory =
            LoggerFactory.Create(fun builder -> builder.AddSerilog() |> ignore)

        let connectionString = config.ConnectionString

        services.AddDbContext<DbDataContext>
            ((fun o ->
                let o' =
                    o.UseLoggerFactory(loggerFactory).EnableSensitiveDataLogging true

                NpgsqlDbContextOptionsExtensions.UseNpgsql
                    (o',
                     connectionString,
                     (fun b ->
                         b.MigrationsAssembly("PRR.Data.DataContextMigrations")
                         |> ignore))
                |> ignore))
        |> ignore
