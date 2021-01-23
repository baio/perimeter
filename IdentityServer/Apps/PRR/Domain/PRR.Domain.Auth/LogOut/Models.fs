namespace PRR.Domain.Auth.LogOut

open Common.Domain.Models
open PRR.Data.DataContext
open PRR.System.Models
open System.Threading.Tasks
open Microsoft.Extensions.Logging

[<AutoOpen>]
module Models =

    [<CLIMutable>]
    type Data =
        { AccessToken: Token
          ReturnUri: string }

    type Result = { ReturnUri: string }

    type OnSuccess = UserId -> Task<unit>

    type Env =
        { DataContext: DbDataContext
          AccessTokenSecret: string
          OnSuccess: OnSuccess
          Logger: ILogger }

    type LogOut = Env -> Data -> Task<Result>
