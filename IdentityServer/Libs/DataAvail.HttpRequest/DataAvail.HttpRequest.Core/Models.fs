namespace DataAvail.HttpRequest.Core

[<AutoOpen>]
module Models =

    open System.Threading.Tasks

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
