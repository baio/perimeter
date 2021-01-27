namespace PRR.Domain.Auth.LogOut

open Common.Domain.Models
open DataAvail.KeyValueStorage.Core
open PRR.Data.DataContext
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
          KeyValueStorage: IKeyValueStorage
          Logger: ILogger }

    type LogOut = Env -> Data -> Task<Result>
