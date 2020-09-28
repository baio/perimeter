namespace PRR.API.Tests.Utils

open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Npgsql.Logging
open System
open System.IO
open Xunit
open PRR.API.Infra
open MongoDB.Driver

[<AutoOpen>]
module Setup =

    let private dropDatabase (connectionString: string) =
        let client = MongoClient(connectionString)
        let dbName = connectionString.Split("/") |> Seq.last
        client.DropDatabase(dbName)

    let recreateDataContext (context: WebHostBuilderContext) (services: IServiceCollection) =
        let psqlConnectionString =
            context.Configuration.GetConnectionString "PostgreSQL"
        PRR.Data.DataContextMigrations.DataContextHelpers.RecreateDataContext(psqlConnectionString)
        dropDatabase (context.Configuration.GetConnectionString "MongoJournal")
        dropDatabase (context.Configuration.GetConnectionString "MongoViews")
        ()


    /// Must be used once in single test run (for debugging)
    let setupEFLogging () =
        NpgsqlLogManager.Provider <- EFLogger.ConsoleLoggerProvider()

    let createHost' (resetDb: bool) =
        WebHostBuilder().UseContentRoot(Directory.GetCurrentDirectory()).Configure(Action<_> PRR.API.App.configureApp)
            .ConfigureAppConfiguration(PRR.API.App.configureAppConfiguration)
            .ConfigureServices(Action<_, _> PRR.API.App.configureServices)
        |> fun builder ->
            if resetDb
            then builder.ConfigureServices(Action<_, _> recreateDataContext)
            else builder


    let createServer' =
        createHost' >> (fun x -> new TestServer(x))

    let createServer () = createServer' false
