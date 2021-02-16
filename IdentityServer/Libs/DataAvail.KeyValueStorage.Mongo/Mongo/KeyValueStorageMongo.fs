namespace DataAvail.KeyValueStorage

open System
open System
open MongoDB.Bson
open MongoDB.Driver
open FSharp.Control.Tasks.V2.ContextInsensitive
open DataAvail.KeyValueStorage.Core

[<CLIMutable>]
type DbRecord<'a> =
    { Id: BsonObjectId
      Key: string
      Partition: string
      Data: 'a
      ExpireAt: DateTime
      Tag: string option }

module Mongo =

    let private MongoKeyExistsErrorHResult = -2146233088
    let private MongoKeyNotFoundErrorHResult = -2146233079

    let private createExpireAtIndex () =
        // https://docs.mongodb.com/manual/tutorial/expire-data/
        let expireAt =
            FieldDefinition<DbRecord<obj>>
                .op_Implicit("ExpireAt")

        let builderIndexKeys = Builders<DbRecord<obj>>.IndexKeys

        let indexes = builderIndexKeys.Ascending(expireAt)

        let indexOptions = CreateIndexOptions()
        indexOptions.ExpireAfter <- Nullable(TimeSpan.FromSeconds(float 0))
        CreateIndexModel(indexes, indexOptions)

    let private createFieldPartitionIndex (fieldName, isUniq) =

        let keyFieldDefinition =
            FieldDefinition<DbRecord<obj>>
                .op_Implicit(fieldName)

        let partitionFieldDefinition =
            FieldDefinition<DbRecord<obj>>
                .op_Implicit("Partition")

        let builderIndexKeys = Builders<DbRecord<obj>>.IndexKeys

        let keyIndex =
            builderIndexKeys.Ascending(keyFieldDefinition)

        let partitionIndex =
            builderIndexKeys.Ascending(partitionFieldDefinition)

        let compoundIndex =
            builderIndexKeys.Combine([ keyIndex; partitionIndex ])

        let indexOptions =
            CreateIndexOptions(Unique = (Nullable isUniq))

        CreateIndexModel(compoundIndex, indexOptions)


    let private createKeyIndex () = createFieldPartitionIndex ("Key", true)

    let private createTagIndex () = createFieldPartitionIndex ("Tag", false)

    type KeyValueStorageMongo(connectionString: string, dbName: string, collectionName: string) =
        let connectionString = connectionString
        let client = MongoClient(connectionString)
        let db = client.GetDatabase(dbName)

        member __.CreateIndexes() =
            let collection =
                db.GetCollection<DbRecord<obj>>(collectionName)

            collection.Indexes.CreateMany
                ([ createExpireAtIndex ()
                   createKeyIndex ()
                   createTagIndex () ])

        interface IKeyValueStorage with

            member __.AddValue key v options =
                let collection =
                    db.GetCollection<DbRecord<'a>> collectionName

                let partition = getAddValuePartitionName<'a> options

                task {
                    try
                        do! collection.InsertOneAsync
                                { Id = BsonObjectId(ObjectId.GenerateNewId())
                                  Key = key
                                  Data = v
                                  Partition = partition
                                  ExpireAt = getExpireAtOption options
                                  Tag = getTagOption options }

                        return Result.Ok(())
                    with
                    | :? MongoWriteException as ex when ex.HResult = MongoKeyExistsErrorHResult ->
                        return Result.Error(AddValueError.KeyAlreadyExists)
                    | ex ->
                        return raise ex
                }

            member __.GetValue<'a> key options =
                let collection =
                    db.GetCollection<DbRecord<'a>> collectionName

                let partition = getGetValuePartitionName<'a> options

                task {
                    try
                        let! x =
                            collection
                                .Find(fun x -> x.Key = key && x.Partition = partition)
                                .FirstAsync()
                        // mongo has delay before remove TTL items
                        match x.ExpireAt < DateTime.UtcNow with
                        | true -> return Result.Error(GetValueError.KeyNotFound)
                        | false -> return Result.Ok x.Data
                    with
                    | :? InvalidOperationException as ex when ex.HResult = MongoKeyNotFoundErrorHResult ->
                        return Result.Error(GetValueError.KeyNotFound)
                    | ex -> return raise ex
                }

            member __.RemoveValue<'a> key options =

                let collection =
                    db.GetCollection<DbRecord<_>> collectionName

                let partition = getGetValuePartitionName<'a> options

                task {
                    let! result = collection.DeleteOneAsync(fun doc -> doc.Key = key && doc.Partition = partition)

                    match result.DeletedCount with
                    | 0L -> return Result.Error(RemoveValueError.KeyNotFound)
                    | 1L -> return Result.Ok(())
                    | n -> return raise (InvalidOperationException(sprintf "Too many items to delete %i" n))
                }

            member __.RemoveValuesByTag<'a> tag options =

                let collection =
                    db.GetCollection<DbRecord<_>> collectionName

                let partition = getGetValuePartitionName<'a> options

                task {
                    let! _ = collection.DeleteManyAsync(fun doc -> doc.Tag = (Some tag) && doc.Partition = partition)
                    return ()
                }
