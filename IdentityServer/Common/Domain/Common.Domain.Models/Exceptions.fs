namespace Common.Domain.Models

[<AutoOpen>]
module Exceptions = 

    exception NotFound
    
    exception UnAuthorized of string option

    exception Forbidden

    exception Conflict of string
        
    exception BadRequest of BadRequestError array