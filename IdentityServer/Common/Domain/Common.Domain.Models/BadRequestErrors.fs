namespace Common.Domain.Models

[<AutoOpen>]
module BadRequestErrors =
    type BadRequestFieldError =
        | EMPTY_VALUE
        | EMPTY_STRING
        | NOT_CONTAINS_STRING of string
        | NOT_CONTAINS_ALL_STRING of string
        | NOT_IN_RANGE of (int * int)
        | NOT_URLS_LIST of string seq
        | NOT_URL_STRING
        | NOT_DOMAIN_NAME
        | NOT_EMAIL_STRING
        | MAX_LENGTH of int
        | MIN_LENGTH of int
        | MISS_UPPER_LETTER
        | MISS_LOWER_LETTER
        | MISS_SPECIAL_CHAR
        | MISS_DIGIT
        | PASSWORD
        | CUSTOM of string
    
    type BadRequestCommonError = string
    
    type BadRequestError =
        | BadRequestFieldError of string * BadRequestFieldError
        | BadRequestCommonError of BadRequestCommonError
                     