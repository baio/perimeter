namespace DataAvail.KeyValueStorage

open System
open MongoDB.Bson
open MongoDB.Driver
open FSharp.Control.Tasks.V2.ContextInsensitive
open PartitionName

[<CLIMutable>]
type DbRecord<'a> =
    { Id: BsonObjectId
      Key: string
      Partition: string
      Data: 'a
      ExpireAt: DateTime }

module Mongo =

    let private MongoKeyExistsErrorHResult = -2146233088
    let private MongoKeyNotFoundErrorHResult = -2146233079

    let private createExpireAtIndex (collection: IMongoCollection<DbRecord<obj>>) =
        // https://docs.mongodb.com/manual/tutorial/expire-data/
        let expireAt =
            FieldDefinition<DbRecord<obj>>.op_Implicit("ExpireAt")

        let builderIndexKeys = Builders<DbRecord<obj>>.IndexKeys

        let indexes = builderIndexKeys.Ascending(expireAt)

        let indexOptions = CreateIndexOptions()
        indexOptions.ExpireAfter <- Nullable(TimeSpan.FromSeconds(float 0))
        let indexKeyDefinition = CreateIndexModel(indexes, indexOptions)
        collection.Indexes.CreateOneAsync(indexKeyDefinition)

    let private createKeyIndex (collection: IMongoCollection<DbRecord<obj>>) =

        let keyFieldDefinition =
            FieldDefinition<DbRecord<obj>>.op_Implicit("Key")

        let partitionFieldDefinition =
            FieldDefinition<DbRecord<obj>>.op_Implicit("Partition")

        let builderIndexKeys = Builders<DbRecord<obj>>.IndexKeys

        let keyIndex =
            builderIndexKeys.Ascending(keyFieldDefinition)

        let partitionIndex =
            builderIndexKeys.Ascending(partitionFieldDefinition)

        let compoundIndex =
            builderIndexKeys.Combine([ keyIndex; partitionIndex ])

        let indexOptions =
            CreateIndexOptions(Unique = (Nullable true))

        collection.Indexes.CreateOneAsync(compoundIndex, indexOptions)


    type KeyValueStorageMongo(connectionString: string, dbName: string, collectionName: string) =
        let connectionString = connectionString
        let client = MongoClient(connectionString)
        let db = client.GetDatabase(dbName)

        member __.CreateIndexes() =
            let collection =
                db.GetCollection<DbRecord<obj>>(collectionName)

            createExpireAtIndex collection
            createKeyIndex collection

        interface IKeyValueStorage with
            member __.AddValue key v expiresAt =
                let collection =
                    db.GetCollection<DbRecord<'a>> collectionName

                let partition = getPartitionName<'a> ()

                task {
                    try
                        do! collection.InsertOneAsync
                                { Id = BsonObjectId.Empty
                                  Key = key
                                  Data = v
                                  Partition = partition
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

                let partition = getPartitionName<'a> ()

                task {
                    try
                        let! x = collection.Find(fun x -> x.Key = key && x.Partition = partition).FirstAsync()
                        // mongo has delay before remove TTL items
                        match x.ExpireAt < DateTime.UtcNow with
                        | true -> return Result.Error(GetValueError.KeyNotFound)
                        | false -> return Result.Ok x.Data
                    with
                    | :? InvalidOperationException as ex when ex.HResult = MongoKeyNotFoundErrorHResult ->
                        return Result.Error(GetValueError.KeyNotFound)
                    | ex -> return raise ex
                }

            member __.RemoveValue<'a> key =

                let collection =
                    db.GetCollection<DbRecord<_>> collectionName

                let partition = getPartitionName<'a> ()

                task {
                    let! result = collection.DeleteOneAsync(fun doc -> doc.Key = key && doc.Partition = partition)

                    match result.DeletedCount with
                    | 0L -> return Result.Error(RemoveValueError.KeyNotFound)
                    | 1L -> return Result.Ok(())
                    | n -> return raise (InvalidOperationException(sprintf "Too many items to delete %i" n))
                }
