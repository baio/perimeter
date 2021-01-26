namespace Tests

open DataAvail.KeyValueStorage.Core
open DataAvail.KeyValueStorage.Mongo
open Microsoft.Extensions.Configuration
open MongoDB.Driver
open FSharp.Control.Tasks.V2.ContextInsensitive

[<AutoOpen>]
module SetUp =

    let setUp () =

        let configuration =
            ConfigurationBuilder().AddJsonFile("appsettings.json").Build()

        let connectionString =
            configuration.Item("Mongo:ConnectionString")

        let dbName = configuration.Item("Mongo:DbName")

        let collectionName =
            configuration.Item("Mongo:CollectionName")

        let client = MongoClient(connectionString)

        client.DropDatabase(dbName)

        let keyValueStorage =
            KeyValueStorageMongo(connectionString, dbName, collectionName)

        let _ = keyValueStorage.CreateIndexes()

        keyValueStorage :> IKeyValueStorage
