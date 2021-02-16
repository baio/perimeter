namespace Tests

open System
open DataAvail.KeyValueStorage.Core
open MongoDB.Bson
open MongoDB.Bson
open MongoDB.Bson.Serialization.Attributes
open MongoDB.Driver
open Xunit
open Xunit.Priority
open FSharp.Control.Tasks.V2.ContextInsensitive

[<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
module CommonTests =

    type Union =
        | One
        | Two

    [<CLIMutable>]
    type Data =
        { Id: BsonObjectId
          Key: string
          Union: Union }

    let mutable storage: IMongoCollection<Data> = null

    [<Fact>]
    let ``A. Before all`` () =
        let result = setUp' ()
        storage <- result.Collection

    [<Fact>]
    let ``B. Discrete union test`` () =
        let data =
            { Id = BsonObjectId.Empty
              Union = Two
              Key = "two" }

        storage.InsertOne data

        let data2 =
            storage.Find(fun x -> x.Key = "two").Single()

        Assert.Equal(data.Union, data2.Union)
