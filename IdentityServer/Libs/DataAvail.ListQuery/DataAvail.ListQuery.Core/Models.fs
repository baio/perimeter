namespace DataAvail.ListQuery.Core

[<AutoOpen>]
module Models =
    type SortOrder =
        | Asc
        | Desc

    type Sort<'s> = { Field: 's; Order: SortOrder }

    type Pager = { Index: int; Size: int }

    type ListQuery<'s, 'f when 'f: comparison> =
        { Sort: Sort<'s> option
          Filters: Map<'f, string>
          Pager: Pager }

    type ListQueryResultPager = { Index: int; Size: int; Total: int64 }

    type ListQueryResult<'i> =
        { Items: 'i seq
          Pager: ListQueryResultPager }
