namespace DataAvail.KeyValueStorage

open System
open System.Threading.Tasks

[<AutoOpen>]
module Core =

    type AddValueError =
        | KeyAlreadyExists
        | ExpiredAtLessThanCurrent

    type GetValueError =
        | KeyNotFound
        | Expired

    type RemoveValueError =
        | KeyNotFound
        | Expired

    type IKeyValueStorage =
        abstract member AddValue: key:string -> v:'a -> expiredAt:DateTime -> Task<Result<unit, AddValueError>>
        abstract member GetValue<'a> : key:string -> Task<Result<'a, GetValueError>>
        abstract member RemoveValue: key:string -> Task<Result<unit, RemoveValueError>>
