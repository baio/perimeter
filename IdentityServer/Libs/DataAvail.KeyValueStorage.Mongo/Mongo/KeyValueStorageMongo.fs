namespace DataAvail.KeyValueStorage

open System
open System.Threading
open System.Threading.Tasks
open MongoDB.Bson
open MongoDB.Bson
open MongoDB.Bson
open MongoDB.Driver
open MongoDB.Driver
open MongoDB.Driver
open MongoDB.Driver
open MongoDB.Driver.Core.Configuration
open FSharp.Control.Tasks.V2.ContextInsensitive

type DbRecord<'a> =
    { Id: string
      Data: 'a
      ExpireAt: DateTime }


module Mongo =

    let private MongoKeyExistsErrorHResult = -2146233088
    let private MongoKeyNotFoundErrorHResult = -2146233079

    type KeyValueStorageMongo(connectionString: string, dbName: string, collectionName: string) =
        let connectionString = connectionString
        let client = MongoClient(connectionString)
        let db = client.GetDatabase(dbName)

        member __.CreateIndexes() =
            // https://docs.mongodb.com/manual/tutorial/expire-data/
            let collection =
                db.GetCollection<DbRecord<obj>>(collectionName)

            let expireAt =
                FieldDefinition<DbRecord<obj>>.op_Implicit("ExpireAt")

            let builderIndexKeys = Builders<DbRecord<obj>>.IndexKeys

            let indexes = builderIndexKeys.Ascending(expireAt)

            let indexOptions = CreateIndexOptions()
            indexOptions.ExpireAfter <- Nullable(TimeSpan.FromSeconds(float 0))
            let indexKeyDefinition = CreateIndexModel(indexes, indexOptions)
            collection.Indexes.CreateOneAsync(indexKeyDefinition)

        interface IKeyValueStorage with
            member __.AddValue key v expiresAt =
                let collection =
                    db.GetCollection<DbRecord<'a>> collectionName

                task {
                    try
                        do! collection.InsertOneAsync
                                { Id = key
                                  Data = v
                                  ExpireAt = expiresAt }

                        return Result.Ok(())
                    with
                    | :? MongoWriteException as ex when ex.HResult = MongoKeyExistsErrorHResult ->
                        return Result.Error(AddValueError.KeyAlreadyExists)
                    | ex -> return raise ex
                }

            member __.GetValue<'a> key =
                let collection =
                    db.GetCollection<DbRecord<'a>> collectionName

                task {
                    try
                        let! x = collection.Find(fun x -> x.Id = key).FirstAsync()
                        // mongo has delay before remove TTL items
                        match x.ExpireAt < DateTime.UtcNow with
                        | true -> return Result.Error(GetValueError.KeyNotFound)
                        | false -> return Result.Ok x.Data
                    with
                    | :? InvalidOperationException as ex when ex.HResult = MongoKeyNotFoundErrorHResult ->
                        return Result.Error(GetValueError.KeyNotFound)
                    | ex -> return raise ex
                }

            member __.RemoveValue key =

                let collection =
                    db.GetCollection<DbRecord<_>> collectionName

                task {
                    let! result = collection.DeleteOneAsync(fun doc -> doc.Id = key)

                    match result.DeletedCount with
                    | 0L -> return Result.Error(RemoveValueError.KeyNotFound)
                    | 1L -> return Result.Ok(())
                    | n -> return raise (InvalidOperationException(sprintf "Too many items to delete %i" n))
                }
