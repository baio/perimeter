namespace Common.Domain.Models

[<AutoOpenAttribute>]
module ListQuery =

    type SortOrder =
        | Asc
        | Desc

    type Sort<'s> =
        { Field: 's
          Order: SortOrder }

    type Pager =
        { Index: int
          Size: int }
    
    type ListQuery<'s, 'f when 'f: comparison> =
        { Sort: Sort<'s> option
          Filters: Map<'f, string>
          Pager: Pager }
