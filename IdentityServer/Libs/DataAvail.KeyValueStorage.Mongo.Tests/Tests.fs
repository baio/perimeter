namespace Tests

open System.Configuration
open System.Threading
open DataAvail.KeyValueStorage.Core
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


    [<PartitionName("DataA")>]
    type Data = { SomeField: string }

    [<Fact>]
    let ``A. Before all`` () =

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
    let ``B. Create Indexes`` () =
        task {
            let! _ = (storage.Value :?> KeyValueStorageMongo).CreateIndexes()
            Assert.True(true)
        }

    [<Fact>]
    let ``C. Add item to storage should be success`` () =
        task {
            let! _ = storage.Value.AddValue "one" { SomeField = "test" } (DateTime.Now.AddSeconds(float 100))
            Assert.True(true)
        }


    [<Fact>]
    let ``D. Add item with the same key should give KeyAlreadyExists error`` () =
        task {
            let! result = storage.Value.AddValue "one" { SomeField = "test" } DateTime.Now
            Assert.Equal(Result.Error(KeyAlreadyExists), result)
        }


    [<Fact>]
    let ``E.A Retrieve item from storage should be success`` () =
        task {
            let! result = storage.Value.GetValue<Data> "one"
            Assert.Equal(Result.Ok({ SomeField = "test" }), result)
        }

    [<Fact>]
    let ``E.B Read not existent item should give KeyNotFound`` () =
        task {
            let! result = storage.Value.GetValue<Data> "two"
            Assert.Equal(Result.Error(GetValueError.KeyNotFound), result)
        }

    [<Fact>]
    let ``F. Add short lived item and then retrieve it should fail with KeyNotFound`` () =
        task {
            let! _ = storage.Value.AddValue "short" { SomeField = "short lived item" } (DateTime.Now)
            Thread.Sleep(1)
            let! result = storage.Value.GetValue<Data> "short"
            Assert.Equal(Result.Error(GetValueError.KeyNotFound), result)
        }

    [<Fact>]
    let ``J. Remove item should success`` () =
        task {
            let! result = storage.Value.RemoveValue<Data> "one"
            Assert.Equal(Result.Ok(()), result)
        }

    [<Fact>]
    let ``K. Get removed item should fail with KeyNotFound error`` () =
        task {
            let! result = storage.Value.GetValue<Data> "one"
            Assert.Equal(Result.Error(GetValueError.KeyNotFound), result)
        }

    [<Fact>]
    let ``L. Remove not existent item should fail with KeyNotFound error`` () =
        task {
            let! result = storage.Value.RemoveValue<Data> "one"
            Assert.Equal(Result.Error((RemoveValueError.KeyNotFound)), result)
        }
