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

[<AutoOpen>]
module Setup =
    
    let recreateDataContext (context: WebHostBuilderContext) (services: IServiceCollection) =
        let connectionString =
            context.Configuration.GetConnectionString "PostgreSQL"
            
        let storageConnectionString = context.Configuration.GetValue("Test:AzureStorage:ConnectionString")            
        let storageBlobContainer = context.Configuration.GetValue("Test:AzureStorage:BlobContainer")
        let storageTable = context.Configuration.GetValue("Test:AzureStorage:Table")       
            
        PRR.DATA.DataContextMigrations.DataContextHelpers.RecreateDataContext(connectionString)
        Common.Test.Utils.AzureBlob.removeBlobContainer storageConnectionString storageBlobContainer
        Common.Test.Utils.AzureBlob.removeTable storageConnectionString storageTable
        ()
        

    /// Must be used once in single test run (for debugging)
    let setupEFLogging () =
        NpgsqlLogManager.Provider <- EFLogger.ConsoleLoggerProvider()

    let createHost' (resetDb: bool) =
        WebHostBuilder().UseContentRoot(Directory.GetCurrentDirectory()).Configure(Action<_> PRR.API.App.configureApp)
            .ConfigureAppConfiguration(PRR.API.App.configureAppConfiguration)
            .ConfigureServices(Action<_, _> PRR.API.App.configureServices)
        |> fun builder ->        
            if resetDb then builder.ConfigureServices(Action<_, _> recreateDataContext)
            else builder


    let createServer' = createHost' >> (fun x -> new TestServer(x))

    let createServer() = createServer' false
