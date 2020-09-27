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

    let recreateDataContext (context: WebHostBuilderContext) (services: IServiceCollection) =
        let psqlConnectionString =
            context.Configuration.GetConnectionString "PostgreSQL"

        let mongoJournalConnectionString =
            context.Configuration.GetConnectionString "MongoJournal"

        let dbName =
            mongoJournalConnectionString.Split("/")
            |> Seq.last

        PRR.Data.DataContextMigrations.DataContextHelpers.RecreateDataContext(psqlConnectionString)

        let client =
            MongoClient(mongoJournalConnectionString)

        client.DropDatabase(dbName)
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
