namespace Common.Domain.Utils

open Common.Domain.Models
open System

[<AutoOpen>]
module BadRequestValidators = 
    let ofBool x =
        function
        | true -> Some x
        | false -> None

    let noneIf f x =
        if f x then Some x
        else None

    let noneIfNullOrEmpty = noneIf String.IsNullOrEmpty

    let validateNullOrEmpty name =
        String.IsNullOrEmpty >> ofBool (name, EMPTY_STRING) >> Option.map BadRequestFieldError

    let validateMaxLength' max name =
        String.length
        >> (fun x -> x > max)
        >> ofBool (name, MAX_LENGTH max)
        >> Option.map BadRequestFieldError

    let validateMaxLength max name =
        noneIfNullOrEmpty >> Option.bind (validateMaxLength' max name)
