namespace Common.Domain.Models

[<AutoOpen>]
module ListResult =

    type Pager =
        { Index: int
          Size: int
          Total: int64 }

    type ListResponse<'a> =
        { Items: 'a seq
          Pager: Pager }
