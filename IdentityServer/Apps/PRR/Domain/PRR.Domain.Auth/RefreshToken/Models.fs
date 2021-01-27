namespace PRR.Domain.Auth.RefreshToken

open Common.Domain.Models
open DataAvail.KeyValueStorage.Core
open PRR.Data.DataContext
open PRR.Domain.Auth
open PRR.Domain.Auth.LogInToken
open System
open System.Threading.Tasks
open Microsoft.Extensions.Logging

[<AutoOpen>]
module Models =

    type Data = { RefreshToken: Token }

    type Env =
        { DataContext: DbDataContext
          JwtConfig: JwtConfig
          Logger: ILogger
          HashProvider: HashProvider
          KeyValueStorage: IKeyValueStorage
          TokenExpiresIn: int<minutes> }

    type RefreshToken = Env -> Token -> Data -> Task<Result>
