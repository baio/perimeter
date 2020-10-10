namespace FSharp.MongoDB.Driver

open MongoDB.Bson
open MongoDB.Driver
open FSharp.MongoDB.Driver
open Microsoft.FSharp.Quotations

[<AutoOpen>]
module FindHelpers =

    let find' (col: IMongoCollection<'a>) (query: BsonDocument) (sort: BsonDocument option) =
        let filterDefinition = FilterDefinition.op_Implicit (query)
        let findResult = col.Find(filterDefinition)
        match sort with
        | Some sort ->
            let sortDefinition = SortDefinition.op_Implicit (sort)
            findResult.Sort(sortDefinition)
        | None -> findResult

    type FindSort<'a, 'b> =
        | FindSortAsc of Expr<'a -> 'b>
        | FindSortDesc of Expr<'a -> 'b>
        | FindSortNone

    let find (col: IMongoCollection<'a>) (query: Expr<'a -> 'b> option) (sort: FindSort<'a, 'c>) =

        let query' =
            match query with
            | Some query -> bson query
            | None -> BsonDocument()

        let sort' =
            match sort with
            | FindSortAsc expr -> bsonOrder expr BsonOrderAsc |> Some
            | FindSortDesc expr -> bsonOrder expr BsonOrderDesc |> Some
            | FindSortNone -> None

        find' col query' sort'

    let toList (fluent: IFindFluent<'a, 'b>) = fluent.ToList()

    let toListAsync (fluent: IFindFluent<'a, 'b>) = fluent.ToListAsync()

    let countAsync (fluent: IFindFluent<'a, 'b>) = fluent.CountDocumentsAsync()

    let skip (i: int) (fluent: IFindFluent<'a, 'b>) = fluent.Limit(System.Nullable i)

    let limit (i: int) (fluent: IFindFluent<'a, 'b>) = fluent.Limit(System.Nullable i)

    let first (fluent: IFindFluent<'a, 'b>) = (fluent).First()

    let firstAsync (fluent: IFindFluent<'a, 'b>) = (fluent).FirstAsync()

    let firstOrDefault (fluent: IFindFluent<'a, 'b>) = (fluent).FirstOrDefault()

    let firstOrDefaultAsync (fluent: IFindFluent<'a, 'b>) = (fluent).FirstOrDefaultAsync()

    type SortBy<'a, 'b> =
        | SortByAsc of Expr<'a -> 'b>
        | SortByDesc of Expr<'a -> 'b>

    let sortBy (col: IMongoCollection<'a>) =
        function
        | SortByAsc expr -> find col None (FindSortAsc expr) |> project expr
        | SortByDesc expr -> find col None (FindSortDesc expr) |> project expr

    let maxBy' col expr = sortBy col (SortByDesc expr)

    let maxBy col expr = maxBy' col expr |> firstOrDefault

    let maxByAsync col expr = maxBy' col expr |> firstAsync
    
    let isDocumentEmpty (doc: BsonDocument) =
        FilterDefinition.Empty.ToBsonDocument().Equals(doc)
       
    let andWhere (expr: Expr<'b -> bool>) (fluent: IFindFluent<'a, 'b>): IFindFluent<'a, 'b> =
        let left = fluent.Filter.ToBsonDocument()
        let right = bson expr
        if isDocumentEmpty left then
            fluent.Filter <- FilterDefinition.op_Implicit right
        else
            let doc =
                BsonDocument("$and", BsonArray([ left; right ]))
            fluent.Filter <- FilterDefinition.op_Implicit doc            
        fluent
