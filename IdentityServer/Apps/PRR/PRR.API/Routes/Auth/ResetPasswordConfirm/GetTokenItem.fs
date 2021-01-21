namespace PRR.API.Routes.Auth.ResetPasswordConfirm

open Common.Domain.Giraffe
open PRR.API
open PRR.System.Models

[<AutoOpen>]
module private GetTokenItem =

    let getTokenItem ctx token =
        (token
         |> (bindSysQuery (ResetPassword.GetToken >> Queries.ResetPassword))) ctx
