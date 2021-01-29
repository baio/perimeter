namespace FSharp.MongoDB.Driver

open System.Linq.Expressions
open Microsoft.FSharp.Linq.RuntimeHelpers
open MongoDB.Bson
open MongoDB.Driver
open FSharp.MongoDB.Driver
open Microsoft.FSharp.Quotations

[<AutoOpen>]
module ProjectionHelpers =

    let private toExpression<'a, 'b> (``f# lambda``: Quotations.Expr<'a>) =
        ``f# lambda``
        |> LeafExpressionConverter.QuotationToExpression
        |> unbox<Expression<'b>>


    let project (projection: Expr<'b -> 'c>) (fluent: IFindFluent<'a, 'b>) =
        let expr' =
            <@ System.Func<'b, 'c>(fun b -> ((%projection) b)) @>
            |> toExpression

        fluent.Project<'a, 'b, 'c>(expr')
