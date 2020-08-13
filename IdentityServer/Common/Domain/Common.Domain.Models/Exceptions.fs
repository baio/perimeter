namespace Common.Domain.Models

[<AutoOpen>]
module Exceptions = 

    exception NotFound
    
    exception UnAuthorized of string option
    
    let UnAuthorized' = UnAuthorized None
    
    let unAuthorized = Some >> UnAuthorized       

    exception Forbidden

    exception Conflict of ConflictError
        
    exception BadRequest of BadRequestError array