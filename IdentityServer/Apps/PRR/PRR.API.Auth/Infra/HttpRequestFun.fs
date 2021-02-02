namespace PRR.API.Auth.Infra

open DataAvail.HttpRequest.Core
open PRR.API.Auth.Infra

[<AutoOpen>]
module HttpRequestFun =


    type HttpRequestFunProvider(httpRequestFun: HttpRequestFun) =
        interface IHttpRequestFunProvider with
            member __.HttpRequestFun = httpRequestFun
