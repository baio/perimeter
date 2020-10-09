namespace FSharp.MongoDB.Driver

open Microsoft.FSharp.Quotations
open Microsoft.FSharp.Quotations.Patterns
open Microsoft.FSharp.Quotations.DerivedPatterns
open MongoDB.Bson

[<AutoOpen>]
module BsonOrder =

    type BsonOrder =
        | BsonOrderAsc
        | BsonOrderDesc

    let bsonOrder (expr: Expr<'a -> 'b>) order =
        let rec sort expr =
            match expr with
            | Lambda (_, body) -> sort body
            | PropertyGet (_, propOrValInfo, _) ->
                BsonDocument(propOrValInfo.Name, BsonValue.Create(if order = BsonOrderAsc then 1 else -1))
            | _ -> failwith "Unknown sort expression"

        sort expr
