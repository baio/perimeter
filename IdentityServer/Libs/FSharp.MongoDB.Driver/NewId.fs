namespace DataAvail.KeyValueStorage.Mongo.Tests

open MongoDB.Bson

[<AutoOpenAttribute>]
module NewId =
    let newId () = BsonObjectId(ObjectId.GenerateNewId())
