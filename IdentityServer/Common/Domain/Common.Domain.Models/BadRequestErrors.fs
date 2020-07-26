namespace Common.Domain.Models

[<AutoOpen>]
module BadRequestErrors =
    type BadRequestFieldError =
        | EMPTY_STRING
        | MAX_LENGTH of int
    
    type BadRequestCommonError = string
    
    type BadRequestError =
        | BadRequestFieldError of string * BadRequestFieldError
        | BadRequestCommonError of BadRequestCommonError
                     