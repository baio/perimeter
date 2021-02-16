namespace DataAvail.Common

[<AutoOpen>]
module Tuples =

    let doublet a b = (a, b)

    let triplet a b c = (a, b, c)

    let tripletIgnore _ _ _ = ()

    let quadrolet a b c d = (a, b, c, d)

    let inline ofSysTuple (x: System.Tuple<'a, 'b>) = (x.Item1, x.Item2)

    let inline ofSysTriple (x: System.Tuple<'a, 'b, 'c>) = (x.Item1, x.Item2, x.Item3)

    let tryFindTupleValue' fn key =
        Seq.tryFind (fun (k, _) -> k = key)
        >> Option.map snd
        >> Option.bind fn

    let tryFindTupleValue key =
        Seq.tryFind (fun (k, _) -> k = key)
        >> Option.map snd

    let tryFindTupleInt x = x |> tryFindTupleValue' tryParseInt
    let tryFindTupleBoolean x = x |> tryFindTupleValue' tryParseBoolean
