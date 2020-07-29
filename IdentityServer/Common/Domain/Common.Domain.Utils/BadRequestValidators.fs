namespace Common.Domain.Utils

open Common.Domain.Models
open FSharpx
open System
open System.Text.RegularExpressions

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
        String.IsNullOrEmpty
        >> ofBool (name, EMPTY_STRING)
        >> Option.map BadRequestFieldError

    let validateRegex name err (regex: string) =
        (Regex(regex).IsMatch)
        >> not
        >> ofBool (name, err)
        >> Option.map BadRequestFieldError

    let validateMaxLength max name =
        String.length
        >> (fun x -> x > max)
        >> ofBool (name, MAX_LENGTH max)
        >> Option.map BadRequestFieldError

    let validateMinLength min name =
        String.length
        >> (fun x -> x < min)
        >> ofBool (name, MIN_LENGTH min)
        >> Option.map BadRequestFieldError


