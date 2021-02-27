namespace PRR.Domain.Auth.LogIn.RefreshToken

open PRR.Domain.Auth.LogIn.Common
open PRR.Domain.Models
open DataAvail.KeyValueStorage.Core
open PRR.Data.DataContext
open PRR.Domain.Auth
open System
open System.Threading.Tasks
open Microsoft.Extensions.Logging

[<AutoOpen>]
module Models =

    type Data =
        { Grant_Type: string
          Refresh_Token: Token }

    type Env =
        { DataContext: DbDataContext
          JwtConfig: JwtConfig
          Logger: ILogger
          HashProvider: HashProvider
          KeyValueStorage: IKeyValueStorage
          TokenExpiresIn: int<minutes> }

    type RefreshToken = Env -> Token -> Data -> Task<LogInResult>
