﻿namespace Common.Domain.Utils

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

    let noneIf f x = if f x then Some x else None

    let isEmpty = String.IsNullOrEmpty

    let noneIfNullOrEmpty = noneIf isEmpty

    let private seqJoin = String.concat ","

    let validateDomainName name =
        (Regex("^[a-z-0-9]+$").IsMatch)
        >> not
        >> ofBool (name, NOT_DOMAIN_NAME)
        >> Option.map BadRequestFieldError

    let validateNullOrEmpty name =
        String.IsNullOrEmpty
        >> ofBool (name, EMPTY_STRING)
        >> Option.map BadRequestFieldError

    let validateNull name x =
        if x = null
        then Some(BadRequestFieldError(name, EMPTY_VALUE))
        else None

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
        >> ofBool (name, list |> seqJoin |> NOT_CONTAINS_STRING)
        >> Option.map BadRequestFieldError

    let validateContainsAll' (list: string seq) name =
        Seq.except list
        >> Seq.isEmpty
        >> ofBool (name, list |> seqJoin |> NOT_CONTAINS_ALL_STRING)
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
                false
            with :? FormatException -> true)
        >> ofBool (name, NOT_EMAIL_STRING)
        >> Option.map BadRequestFieldError

    let skipEmpty str f = if isEmpty str then None else f str

    let skipNull x f = if x = null then None else f x

    let validateEmail name str = validateEmail' name |> skipEmpty str

    let validateUrl name str = validateUrl' name |> skipEmpty str

    let validateContains list name str =
        validateContains' list name |> skipEmpty str

    let validateContainsAll list name x =
        validateContainsAll' list name |> skipNull x
