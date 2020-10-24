namespace Common.Utils

[<AutoOpen>]
module StringUtils =
    open Common.Utils
    open System
    open System.Text.RegularExpressions

    let trimStart (x: string) (s: string) = s.Replace(x, "")
    let startsWith (x: string) (s: string) = s.StartsWith(x)

    let tryParseInt (x: string) =
        match System.Int32.TryParse x with
        | true, int -> Some int
        | _ -> None

    let (|Regex|_|) pattern input =
        let m = Regex.Match(input, pattern)
        if m.Success
        then Some(List.tail [ for g in m.Groups -> g.Value ])
        else None

    let (|Int|_|) = tryParseInt

    let splitName (name: string) =
        match name.Split " " with
        | [| a; b |] -> (a, b)
        | [| a |] -> (a, null)
        | [||] -> (null, null)
        | x -> (x.[0], x.[1..] |> String.concat " ")
