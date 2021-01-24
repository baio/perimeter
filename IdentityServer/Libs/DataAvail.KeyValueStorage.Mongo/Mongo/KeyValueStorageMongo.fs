namespace DataAvail.KeyValueStorage

open System
open System.Threading
open System.Threading.Tasks
open MongoDB.Bson
open MongoDB.Bson
open MongoDB.Bson
open MongoDB.Driver
open MongoDB.Driver
open MongoDB.Driver.Core.Configuration
open FSharp.Control.Tasks.V2.ContextInsensitive

type DbRecord<'a> =
    { Id: string
      Data: 'a
      ExpiredAt: DateTime }


module Mongo =

    type KeyValueStorageMongo(connectionString: string, dbName: string, collectionName: string) =
        let connectionString = connectionString
        let client = MongoClient(connectionString)
        let db = client.GetDatabase(dbName)

        interface IKeyValueStorage with
            member __.AddValue key v expiredAt =
                let collection =
                    db.GetCollection<DbRecord<'a>> collectionName

                task {
                    do! collection.InsertOneAsync
                            { Id = key
                              Data = v
                              ExpiredAt = expiredAt }
                    return Result.Ok(())
                }

            member __.GetValue<'a> key =
                let collection =
                    db.GetCollection<DbRecord<'a>> collectionName


                task {
                    let! x = collection.Find(fun x -> x.Id = key).FirstAsync()
                    return Result.Ok(x.Data)
                }

            member __.RemoveValue key = Task.FromResult(Result.Ok(()))
