namespace PRR.API.Routes.Auth.SignUpConfirm

open Common.Domain.Giraffe
open PRR.API
open PRR.System.Models

[<AutoOpen>]
module private GetTokenItem =

    let getTokenItem ctx token =
        (token
         |> (bindSysQuery (SignUpToken.GetToken >> Queries.SignUpToken))) ctx
