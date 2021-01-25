namespace Tests

open System
open DataAvail.KeyValueStorage
open Xunit
open Xunit.Priority
open FSharp.Control.Tasks.V2.ContextInsensitive

[<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
module TagTests =

    let mutable storage: IKeyValueStorage option = None

    [<PartitionName("DataT")>]
    type Data = { Name: string }

    [<Fact>]
    let ``A. Before all`` () =
        let kvStorage = setUp ()
        storage <- Some kvStorage

    [<Fact>]
    let ``B. Add item to storage should be success`` () =
        task {
            let! _ = storage.Value.AddValue "one" { Name = "test1" } (DateTime.Now.AddSeconds(float 100)) (Some "1")
            let! _ = storage.Value.AddValue "two" { Name = "test2" } (DateTime.Now.AddSeconds(float 100)) (Some "1")
            Assert.True(true)
        }

    [<Fact>]
    let ``C Retrieve item from storage should be success`` () =
        task {
            let! result = storage.Value.GetValue<Data> "one"
            Assert.Equal(Result.Ok({ Name = "test1" }), result)
            let! result = storage.Value.GetValue<Data> "two"
            Assert.Equal(Result.Ok({ Name = "test2" }), result)
        }

    [<Fact>]
    let ``D Remove items by tag should be success`` () =
        task {
            let! _ = storage.Value.RemoveValuesByTag<Data> "1"
            Assert.True(true)
        }

    [<Fact>]
    let ``E Retrieve item from storage should fails with KeyNotFound`` () =
        task {
            let! result = storage.Value.GetValue<Data> "one"
            Assert.Equal(Result.Error(GetValueError.KeyNotFound), result)
            let! result = storage.Value.GetValue<Data> "two"
            Assert.Equal(Result.Error(GetValueError.KeyNotFound), result)
        }
