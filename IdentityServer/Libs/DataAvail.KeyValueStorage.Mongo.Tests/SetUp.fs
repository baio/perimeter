namespace Tests

open DataAvail.KeyValueStorage.Core
open DataAvail.KeyValueStorage.Mongo
open Microsoft.Extensions.Configuration
open Microsoft.VisualBasic
open MongoDB.Driver
open FSharp.Control.Tasks.V2.ContextInsensitive

[<AutoOpen>]
module SetUp =

    let setUp' () =

        let configuration =
            ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build()

        let connectionString =
            configuration.Item("Mongo:ConnectionString")

        let dbName = configuration.Item("Mongo:DbName")

        NamelessInteractive.FSharp.MongoDB.SetUp.registerSerializationAndConventions()

        let collectionName =
            configuration.Item("Mongo:CollectionName")

        let client = MongoClient(connectionString)

        client.DropDatabase(dbName)

        let collection =
            client
                .GetDatabase(dbName)
                .GetCollection(collectionName)

        {| ConnectionString = connectionString
           DbName = dbName
           CollectionName = collectionName
           Collection = collection |}

    let setUp () =

        let result = setUp' ()

        let keyValueStorage =
            KeyValueStorageMongo(result.ConnectionString, result.DbName, result.CollectionName)

        let _ = keyValueStorage.CreateIndexes()
        keyValueStorage :> IKeyValueStorage
