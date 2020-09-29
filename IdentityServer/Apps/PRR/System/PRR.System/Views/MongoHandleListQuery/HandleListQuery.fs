namespace FSharp.Mongo.ListQuery

[<AutoOpen>]
module HandleListQuery =

    open FSharpx.Reader

    let handleListQuery col getFilterFieldExpr getSortFieldExpr prms =
        returnM col
        >>= handleFilter getFilterFieldExpr
        >>= handleSort getSortFieldExpr
        >>= handlePagination
        <| prms
