namespace DataAvail.KeyValueStorage.Core


[<AutoOpen>]
module Utils =
    
    let getExpireAtOption =
        Option.bind (fun (x: AddValueOptions) -> x.ExpiresAt)
        >> Option.defaultValue System.DateTime.MaxValue

    let getTagOption =
        Option.bind (fun (x: AddValueOptions) -> x.Tag |> Option.ofObj)

    let getAddValuePartitionOption =
        Option.bind (fun (x: AddValueOptions) -> x.PartitionName |> Option.ofObj)

    let getGetValuePartitionOption =
        Option.bind (fun (x: GetValueOptions) -> x.PartitionName |> Option.ofObj)

    let getAddValuePartitionName<'a> =
        getAddValuePartitionOption
        >> Option.defaultWith getPartitionName<'a>

    let getGetValuePartitionName<'a> =
        getGetValuePartitionOption
        >> Option.defaultWith getPartitionName<'a>
    

