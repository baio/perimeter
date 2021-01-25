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
          Tag: string
          PartitionName: string }

    let addValueDefaultOptions =
        { ExpiresAt = None
          Tag = null
          PartitionName = null }

    type GetValueOptions = { PartitionName: string }

    let getValueDefaultOptions = { PartitionName = null }

    type IKeyValueStorage =
        abstract AddValue: key:string -> v:'a -> options:AddValueOptions option -> Task<Result<unit, AddValueError>>
        abstract GetValue<'a> : key:string -> options:GetValueOptions option -> Task<Result<'a, GetValueError>>
        abstract RemoveValue<'a> : key:string -> options:GetValueOptions option -> Task<Result<unit, RemoveValueError>>
        abstract RemoveValuesByTag<'a> : tag:string -> options:GetValueOptions option -> Task<unit>
