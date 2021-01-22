namespace PRR.API.Routes.Auth.LogInSSO

open Common.Domain.Giraffe
open PRR.API
open PRR.System.Models

[<AutoOpen>]
module private GetSSOCode =

    let getSSOCode ctx sso =
        (sso |> (bindSysQuery (SSO.GetCode >> Queries.SSO))) ctx
