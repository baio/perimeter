namespace PRR.Domain.Auth.SignUpConfirm

open DataAvail.KeyValueStorage.Core
open Microsoft.Extensions.Logging
open PRR.Data.DataContext
open System.Threading.Tasks

[<AutoOpen>]
module Models =

    type Data = { Token: string }
    type Env =
        { DataContext: DbDataContext
          Logger: ILogger
          KeyValueStorage: IKeyValueStorage }

    type SignUpConfirm = Env -> Data -> Task<int>
