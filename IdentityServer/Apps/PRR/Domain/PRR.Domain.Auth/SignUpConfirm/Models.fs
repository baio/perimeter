namespace PRR.Domain.Auth.SignUpConfirm

open Common.Domain.Models
open DataAvail.KeyValueStorage.Core
open Microsoft.Extensions.Logging
open PRR.Data.DataContext
open PRR.System.Models
open System.Threading.Tasks
open PRR.System.Models.SignUpToken

[<AutoOpen>]
module Models =

    type Data = { Token: string }
    type Env =
        { DataContext: DbDataContext
          Logger: ILogger
          KeyValueStorage: IKeyValueStorage }

    type SignUpConfirm = Env -> Data -> Task<int>
