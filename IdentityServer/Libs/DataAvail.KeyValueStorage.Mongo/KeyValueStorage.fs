namespace DataAvail.KeyValueStorage

open System
open System.Threading.Tasks

[<AutoOpen>]
module Core =

    type PartitionName(name: string) =
        inherit Attribute()
        member __.Name = name

    type AddValueError = KeyAlreadyExists

    type GetValueError = KeyNotFound

    type RemoveValueError = KeyNotFound

    // TODO
    type AddValueOptions =
        { ExpiresAt: DateTime option
          Tag: string option
          PartitionName: string option }
        
    type IKeyValueStorage =
        abstract AddValue: key:string
         -> v:'a -> expiresAt:DateTime -> tag:string option -> Task<Result<unit, AddValueError>>

        abstract GetValue<'a> : key:string -> Task<Result<'a, GetValueError>>
        abstract RemoveValue<'a> : key:string -> Task<Result<unit, RemoveValueError>>
        abstract RemoveValuesByTag<'a> : tag:string -> Task<unit>
