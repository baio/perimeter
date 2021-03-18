namespace DataAvail.Common

open System

[<AutoOpen>]
module StringUtils =

    open System.Text.RegularExpressions

    let isEmpty = System.String.IsNullOrEmpty
    let isNotEmpty = isEmpty >> not

    let emptyDefault x s = if isEmpty s then x else s

    let split (chr: char) (x: string) = x.Split chr

    let concat (chr: char) (x: string seq) = String.concat (chr.ToString()) x

    let splitTuple (chr: char) (x: string) =
        split chr x
        |> (function
        | [| k; v |] -> k, v
        | _ -> raise (ArgumentOutOfRangeException "Not key value"))

    let joinTuple (chr: char) (a, b) = sprintf "%s%c%s" a chr b

    let joinQueryStringTuple = joinTuple '='

    let joinTuples (chr1: char) (chr2: char) = Seq.map (joinTuple chr1) >> concat chr2

    let joinQueryStringTuples x = x |> joinTuples '=' '&'

    let trimStart (x: char) (s: string) = s.TrimStart(x)

    let trimEnd (x: char) (s: string) = s.TrimEnd(x)

    let trim (x: char) (s: string) = s.Trim(x)

    let startsWith (x: string) (s: string) = s.StartsWith(x)

    let toLower (x: string) = x.ToLower()

    let concatUrl x =
        x
        |> Seq.filter isNotEmpty
        |> Seq.map (trim '/')
        |> concat '/'

    let tryParseInt (x: string) =
        match Int32.TryParse x with
        | true, int -> Some int
        | _ -> None

    let tryParseBoolean (x: string) =
        match Boolean.TryParse x with
        | true, int -> Some int
        | _ -> None

    let (|Regex|_|) pattern input =
        let m = Regex.Match(input, pattern)

        if m.Success then
            Some(List.tail [ for g in m.Groups -> g.Value ])
        else
            None

    let (|Int|_|) = tryParseInt

    let splitName (name: string) =
        match name.Split " " with
        | [| a; b |] -> (a, b)
        | [| a |] -> (a, "")
        | [||] -> (null, null)
        | x -> (x.[0], x.[1..] |> String.concat " ")
