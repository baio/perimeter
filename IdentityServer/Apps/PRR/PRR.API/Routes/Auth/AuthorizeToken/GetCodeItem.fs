namespace PRR.API.Routes.Auth.AuthorizeToken

open Common.Domain.Giraffe
open PRR.API
open PRR.System.Models

[<AutoOpen>]
module private GetCodeItem =

    let getCodeItem ctx token =
        (token
         |> (bindSysQuery (LogIn.GetCode >> Queries.LogIn))) ctx
