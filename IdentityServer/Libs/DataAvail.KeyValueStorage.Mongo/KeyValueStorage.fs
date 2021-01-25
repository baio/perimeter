namespace DataAvail.KeyValueStorage

open System
open System.Threading.Tasks

[<AutoOpen>]
module Core =

    type AddValueError = KeyAlreadyExists

    type GetValueError = KeyNotFound

    type RemoveValueError = KeyNotFound

    type IKeyValueStorage =
        abstract AddValue: key:string -> v:'a -> expiresAt: DateTime -> Task<Result<unit, AddValueError>>
        abstract GetValue<'a> : key:string -> Task<Result<'a, GetValueError>>
        abstract RemoveValue: key:string -> Task<Result<unit, RemoveValueError>>
