namespace Common.Domain.Models

[<AutoOpen>]
module ForbiddenError =
    
    type ConflictErrorCode =
        | UNIQUE
                
    type ConflictError =
        | ConflictErrorField of string * ConflictErrorCode
        | ConflictErrorCommon of string


