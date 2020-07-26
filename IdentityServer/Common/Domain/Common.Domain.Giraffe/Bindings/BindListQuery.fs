namespace Common.Domain.Giraffe

[<AutoOpen>]
module BindListQuery =

    open Common.Domain.Models
    open Microsoft.AspNetCore.Http
    open Giraffe  
    open FSharpx.Option
    open Common.Utils
    open System.Text.RegularExpressions

    let getRequestUrl (ctx: HttpContext) =
        ctx.GetRequestUrl()
        
    type SortBinder<'s> = string -> 's option

    type FilterBinder<'f> = string -> 'f option

    let getSortOrder =
        startsWith "-"
        >> function
        | true -> Desc
        | false -> Asc

    let getSort<'s> (ctx: HttpContext) (sortBinder: SortBinder<'s>): Sort<'s> option =
        maybe {
            let! querySort = ctx.TryGetQueryStringValue "sort"
            let! sortField = querySort
                             |> trimStart "-"
                             |> sortBinder
            return { Field = sortField
                     Order = getSortOrder querySort }
        }

    let getFilters<'f when 'f: comparison> (ctx: HttpContext) (filterBinder: FilterBinder<'f>): Map<'f, string> =
        let fqsRegex = Regex("filter\.(\w+)=([^&]*)")
        let queryString = ctx.GetRequestUrl()
        fqsRegex.Matches(queryString)
        |> Seq.choose (fun mat ->
            let value = mat.Groups.[2].Value
            mat.Groups.[1].Value
            |> filterBinder
            |> Option.map (fun field -> field, value))
        |> Map.ofSeq

    let getQueryStringInt (qsRegexPart: string) (defaultValue: int) = 
        getRequestUrl >>
        function
            | Regex (sprintf "%s%s" qsRegexPart "=(\d+)") [index] ->
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
        (ctx: HttpContext) =
        let query: ListQuery<'s, 'f> =
            { Sort = getSort<'s> ctx sortBinder
              Filters = getFilters<'f> ctx filterBinder
              Pager = getPager ctx
            }
        query
