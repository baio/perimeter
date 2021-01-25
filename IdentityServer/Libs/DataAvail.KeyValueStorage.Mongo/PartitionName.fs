namespace DataAvail.KeyValueStorage

open System

module PartitionName =

    let private getStorageNameFromAttribute<'a> () =
        let attrs =
            typeof<'a>.GetCustomAttributes(typeof<PartitionName>, true)
              
        match attrs with
        | [| attr |] -> Some((attr :?> PartitionName).Name)
        | [||] -> None
        | _ -> raise (InvalidOperationException("Multiple StorageName attributes not allowed"))

    let private getStorageNameFromType<'a> () = typeof<'a>.FullName

    let getPartitionName<'a> () =
        match getStorageNameFromAttribute<'a> () with
        | Some name -> name
        | None -> getStorageNameFromType<'a> ()
