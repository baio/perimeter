namespace PRR.API.Infra

open DataAvail.HttpRequest.Core
open PRR.API.Infra

[<AutoOpen>]
module HttpRequestFun =


    type HttpRequestFunProvider(httpRequestFun: HttpRequestFun) =
        interface IHttpRequestFunProvider with
            member __.HttpRequestFun = httpRequestFun
