namespace Common.Utils

[<AutoOpen>]
module Tuples =

    let doublet a b = (a, b)

    let triplet a b c = (a, b, c)

    let tripletIgnore _ _ _ = ()

    let quadrolet a b c d = (a, b, c, d)

    let inline ofSysTuple (x: System.Tuple<'a, 'b>) = (x.Item1, x.Item2)
