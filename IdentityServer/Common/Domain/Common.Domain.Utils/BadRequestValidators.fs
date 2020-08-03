namespace Common.Domain.Utils

open Common.Domain.Models
open FSharpx
open System
open System.Net.Mail
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

    let isEmpty = String.IsNullOrEmpty

    let noneIfNullOrEmpty = noneIf isEmpty

    let private seqJoin = String.concat ","

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

    let validateContains' (list: string seq) name =
        flip (Seq.contains) list
        >> not
        >> ofBool
            (name,
             list
             |> seqJoin
             |> CONTAINS_STRING)
        >> Option.map BadRequestFieldError

    let validateContainsAll (list: string seq) name =
        Seq.except list
        >> Seq.isEmpty
        >> ofBool
            (name,
             list
             |> seqJoin
             |> CONTAINS_ALL_STRING)
        >> Option.map BadRequestFieldError

    let validateUrl' name =
        (Regex("^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$").IsMatch)
        >> not
        >> ofBool (name, NOT_URL_STRING)
        >> Option.map BadRequestFieldError

    let validateEmail' name =
        (fun str ->
        try
            MailAddress(str) |> ignore
            true
        with :? FormatException -> false)
        >> ofBool (name, NOT_URL_STRING)
        >> Option.map BadRequestFieldError

    let skipEmpty str f =
        if isEmpty str then None
        else f str

    let validateEmail name str = validateEmail' name |> skipEmpty str

    let validateUrl name str = validateUrl' name |> skipEmpty str

    let validateContains list name str = validateContains' list name |> skipEmpty str
