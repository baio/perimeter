namespace PRR.API.Routes.Auth.RefreshToken

open Common.Domain.Giraffe
open PRR.API
open PRR.System.Models

[<AutoOpen>]
module private GetTokenItem =

    let getTokenItem ctx sso =
        (sso
         |> (bindSysQuery (RefreshToken.GetToken >> Queries.RefreshToken)))
            ctx
