namespace Common.Domain.Models

[<AutoOpen>]
module BadRequestErrors =
    type BadRequestFieldError =
        | EMPTY_STRING
        | CONTAINS_STRING of string seq
        | CONTAINS_ALL_STRING of string seq
        | NOT_URL_STRING
        | MAX_LENGTH of int
        | MIN_LENGTH of int
        | MISS_UPPER_LETTER
        | MISS_LOWER_LETTER
        | MISS_SPECIAL_CHAR
        | MISS_DIGIT
    
    type BadRequestCommonError = string
    
    type BadRequestError =
        | BadRequestFieldError of string * BadRequestFieldError
        | BadRequestCommonError of BadRequestCommonError
                     