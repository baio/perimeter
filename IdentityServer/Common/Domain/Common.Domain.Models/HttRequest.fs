namespace Common.Domain.Models

open System.Threading.Tasks

[<AutoOpen>]
module HttRequest =

    type HttpRequestMethod =
        | HttpRequestMethodPOST
        | HttpRequestMethodGET

    type HttpRequest =
        { Uri: string
          Method: HttpRequestMethod
          Headers: (string * string) seq
          QueryStringParams: (string * string) seq
          FormBody: (string * string) seq }

    type HttpRequestFun = HttpRequest -> Task<string>
