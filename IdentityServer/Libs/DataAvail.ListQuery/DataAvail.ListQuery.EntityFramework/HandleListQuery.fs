namespace DataAvail.ListQuery.EntityFramework

[<AutoOpen>]
module HandleListQuery =

    open FSharpx.Reader

    let handleListQuery q getFilterFieldExpr getSortFieldExpr prms =
        returnM q
        >>= handleSort getSortFieldExpr
        >>= handleFilter getFilterFieldExpr
        >>= handlePagination
        <| prms
