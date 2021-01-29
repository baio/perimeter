namespace FSharp.MongoDB.Driver

open MongoDB.Driver
open FSharp.MongoDB.Driver

[<AutoOpen>]
module CollectionHelpers =

    let createIndexModel expr order =
        let order = bsonOrder expr order
        CreateIndexModel(IndexKeysDefinition.op_Implicit (order))

    let createIndexesRange (col: IMongoCollection<'a>) keys = col.Indexes.CreateMany(keys)

    let createIndexesRangeAsync (col: IMongoCollection<'a>) keys = col.Indexes.CreateManyAsync(keys)

    let insertOne (col: IMongoCollection<'a>) (doc: 'a) = col.InsertOne doc

    let insertOneAsync (col: IMongoCollection<'a>) (doc: 'a) = col.InsertOneAsync doc
