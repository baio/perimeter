namespace PRR.API.Infra

open Common.Domain.Models
open Newtonsoft.Json
open PRR.API.Infra
open HttpFs.Client
open Hopac

[<AutoOpen>]
module HttpRequestFun =

    let private mapMethod =
        function
        | HttpRequestMethodPOST -> Post
        | HttpRequestMethodGET -> Get

    let httpFsRequestFun: HttpRequestFun =
        fun req ->
            let method = mapMethod req.Method
            job {
                let request = Request.createUrl method req.Uri

                let request =
                    req.Headers
                    |> Seq.fold (fun acc header -> (Request.setHeader (Custom header) acc)) request

                let request =
                    req.QueryStringParams
                    |> Seq.fold (fun acc (k, v) -> (Request.queryStringItem k v) acc) request

                let! response = getResponse request

                let! content = Response.readBodyAsString response

                return content
            }
            |> queueAsTask

    type HttpRequestFunProvider(httpRequestFun: HttpRequestFun) =
        interface IHttpRequestFunProvider with
            member __.HttpRequestFun = httpRequestFun
