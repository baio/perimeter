namespace DataAvail.HttpRequest

open System
open DataAvail.HttpRequest.Core

[<AutoOpen>]
module HttpFs =

    open HttpFs.Client
    open Hopac

    let private mapMethod =
        function
        | HttpRequestMethodPOST -> Post
        | HttpRequestMethodGET -> Get

    let private addFormBody formBody request =
        match Seq.isEmpty formBody with
        | true -> request
        | false ->
            formBody
            |> Seq.map (NameValue)
            |> Seq.toList
            |> BodyForm
            |> Request.body
            |> (fun fn -> fn request)

    let private getHeader (k: string, v: string) =
        match k.ToLower() with
        | "content-type" ->
            match ContentType.parse v with
            | Some ct -> ContentType ct
            | None -> raise (ArgumentException(sprintf "content-type %s not valid" k))
        | _ -> Custom(k, v)

    let httpFsRequestFun: HttpRequestFun =
        fun req ->
            let method = mapMethod req.Method

            job {
                let request = Request.createUrl method req.Uri

                // https://stackoverflow.com/questions/62307624/f-getting-error-httpfs-client-code415-messagecontent-type-not-su
                let request =
                    req.Headers
                    |> Seq.fold (fun acc header ->
                        let header = getHeader header
                        (Request.setHeader header acc)) request

                let request =
                    req.QueryStringParams
                    |> Seq.fold (fun acc (k, v) -> (Request.queryStringItem k v) acc) request

                let request = addFormBody req.FormBody request

                let! response = getResponse request

                let! content = Response.readBodyAsString response

                return content
            }
            |> queueAsTask
