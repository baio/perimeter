namespace PRR.API.Tests.Utils

open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Npgsql.Logging
open System
open System.IO
open MongoDB.Driver

[<AutoOpen>]
module Setup =

    let private dropDatabase (connectionString: string) (dbName: string) =
        let client = MongoClient(connectionString)
        client.DropDatabase(dbName)

    let recreateDataContext (context: WebHostBuilderContext) (services: IServiceCollection) =
        let psqlConnectionString =
            context.Configuration.GetConnectionString "PostgreSQL"

        PRR.Data.DataContextMigrations.DataContextHelpers.RecreateDataContext(psqlConnectionString)

        dropDatabase
            (context.Configuration.GetValue "MongoKeyValueStorage:ConnectionString")
            (context.Configuration.GetValue "MongoKeyValueStorage:DbName")

        dropDatabase
            (context.Configuration.GetValue "MongoViewStorage:ConnectionString")
            (context.Configuration.GetValue "MongoViewStorage:DbName")

        ()


    /// Must be used once in single test run (for debugging)
    let setupEFLogging () =
        NpgsqlLogManager.Provider <- EFLogger.ConsoleLoggerProvider()

    let createAuthHost () =
        WebHostBuilder()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .Configure(Action<_> PRR.API.Auth.App.configureApp)
            .ConfigureAppConfiguration(PRR.API.Auth.App.configureAppConfiguration)
            .ConfigureServices(Action<_, _> PRR.API.Auth.App.configureServices)
        |> fun builder ->            
            builder.ConfigureServices(Action<_, _> recreateDataContext)            

    let createTenantHost () =
        WebHostBuilder()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .Configure(Action<_> PRR.API.Tenant.App.configureApp)
            .ConfigureAppConfiguration(PRR.API.Tenant.App.configureAppConfiguration)
            .ConfigureServices(Action<_, _> PRR.API.Tenant.App.configureServices)

// let createServer' = createHost' >> TestServer

// let createServer () = createServer' false
