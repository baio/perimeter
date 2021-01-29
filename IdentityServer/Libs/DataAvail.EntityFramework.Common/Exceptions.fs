namespace DataAvail.EntityFramework.Common

[<AutoOpen>]
module Exceptions =
    
    exception NotFound

    type ConflictErrorCode =
        | UNIQUE
                
    type ConflictError =
        | ConflictErrorField of string * ConflictErrorCode
        | ConflictErrorCommon of string

    exception Conflict of ConflictError


