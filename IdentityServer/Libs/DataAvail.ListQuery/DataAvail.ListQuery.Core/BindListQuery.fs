namespace DataAvail.ListQuery.Core

[<AutoOpen>]
module BindListQuery =

    open DataAvail.Common
    open Microsoft.AspNetCore.Http
    open Microsoft.AspNetCore.Http.Extensions
    open System.Text.RegularExpressions

    let getRequestUrl (ctx: HttpContext) = ctx.Request.GetEncodedUrl()

    let tryGetQueryStringValue (ctx: HttpContext) (key: string) =
        match ctx.Request.Query.TryGetValue key with
        | true, value -> Some(value.ToString())
        | _ -> None

    type SortBinder<'s> = string -> 's option

    type FilterBinder<'f> = string -> 'f option

    let getSortOrder =
        startsWith "-"
        >> function
        | true -> Desc
        | false -> Asc

    let getSort<'s> (ctx: HttpContext) (sortBinder: SortBinder<'s>): Sort<'s> option =

        Option.maybe {
            let! sort = tryGetQueryStringValue ctx "sort"

            let! field = sort |> trimStart '-' |> sortBinder

            let order = sort |> getSortOrder

            return { Order = order; Field = field }
        }


    let getFilters<'f when 'f: comparison> (ctx: HttpContext) (filterBinder: FilterBinder<'f>): Map<'f, string> =
        let fqsRegex = Regex("filter\.(\w+)=([^&]*)")
        let queryString = getRequestUrl ctx
        fqsRegex.Matches(queryString)
        |> Seq.choose (fun mat ->
            let value = mat.Groups.[2].Value
            mat.Groups.[1].Value
            |> filterBinder
            |> Option.map (fun field -> field, value))
        |> Map.ofSeq

    let getQueryStringInt (qsRegexPart: string) (defaultValue: int) =
        getRequestUrl
        >> function
        | Regex (sprintf "%s%s" qsRegexPart "=(\d+)") [ index ] ->
            match index with
            | Int i -> i
            | _ -> defaultValue
        | _ -> defaultValue

    let getPagerIndex = getQueryStringInt "pager\.index" 0

    let getPagerSize = getQueryStringInt "pager\.size" 25

    let getPager ctx =
        let index = getPagerIndex ctx
        let size = getPagerSize ctx
        { Index = index; Size = size }

    let bindListQuery<'s, 'f when 'f: comparison> ((sortBinder, filterBinder): SortBinder<'s> * FilterBinder<'f>)
                                                  (ctx: HttpContext)
                                                  =
        let query: ListQuery<'s, 'f> =
            { Sort = getSort<'s> ctx sortBinder
              Filters = getFilters<'f> ctx filterBinder
              Pager = getPager ctx }

        query
