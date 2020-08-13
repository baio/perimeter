namespace Common.Domain.Models

[<AutoOpen>]
module ForbiddenError =
    
    type ForbiddenErrorCode =
        | UNIQUE
                
    type ForbiddenError =
        | ForbiddenErrorField of string * ForbiddenErrorCode
        | ForbiddenErrorCommon of string


