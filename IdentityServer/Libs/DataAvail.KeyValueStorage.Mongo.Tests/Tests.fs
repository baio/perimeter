namespace Tests

open System.Configuration
open DataAvail.KeyValueStorage.Core
open DataAvail.KeyValueStorage.Mongo
open System
open Microsoft.Extensions.Configuration
open MongoDB.Driver
open Xunit
open FSharp.Control.Tasks.V2.ContextInsensitive
open Xunit.Priority

[<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
module StorageTests =

    let mutable storage: IKeyValueStorage option = None

    type Data = { SomeField: string }

    [<Fact>]
    let ``1. Before all`` () =

        let configuration =
            ConfigurationBuilder().AddJsonFile("appsettings.json").Build()

        let connectionString =
            configuration.Item("Mongo:ConnectionString")

        let dbName = configuration.Item("Mongo:DbName")

        let collectionName =
            configuration.Item("Mongo:CollectionName")

        let client = MongoClient(connectionString)
        client.DropDatabase(dbName)

        storage <- Some(KeyValueStorageMongo(connectionString, dbName, collectionName) :> IKeyValueStorage)

    [<Fact>]
    let ``2. Add item to storage should be success`` () =

        task {
            let! _ = storage.Value.AddValue "one" { SomeField = "test" } DateTime.Now
            Assert.True(true)
        }

    [<Fact>]
    let ``3. Retrieve item from storage should be success`` () =

        task {
            let! result = storage.Value.GetValue<Data> "one"
            Assert.Equal(result, Result.Ok({ SomeField = "test" }))
        }
