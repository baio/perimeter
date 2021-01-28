namespace DataAvail.ListQuery.Mongo

[<AutoOpen>]
module HandleListQuery =

    open FSharpx.Reader

    let handleListQuery col getFilterFieldExpr getSortFieldExpr prms =
        returnM col
        >>= handleFilter getFilterFieldExpr
        >>= handleSort getSortFieldExpr
        >>= handlePagination
        <| prms
