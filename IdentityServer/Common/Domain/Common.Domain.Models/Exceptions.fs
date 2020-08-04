namespace Common.Domain.Models

[<AutoOpen>]
module Exceptions = 

    exception NotFound

    exception UnAuthorized
    
    exception UnAuthorized' of string

    exception Forbidden

    exception Conflict of string
        
    exception BadRequest of BadRequestError array