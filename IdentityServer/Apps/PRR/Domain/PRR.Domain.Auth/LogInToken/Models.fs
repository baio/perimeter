namespace PRR.Domain.Auth.LogInToken

open Common.Domain.Models

open DataAvail.KeyValueStorage.Core
open PRR.Data.DataContext
open PRR.Domain.Auth
open PRR.System.Models
open System.Threading.Tasks
open Microsoft.Extensions.Logging

[<AutoOpen>]
module Models =
    type Data =
        { Grant_Type: string
          Code: Token
          Redirect_Uri: string
          Client_Id: string
          Code_Verifier: string }

    type Result =
        { id_token: string
          access_token: string
          refresh_token: string }

    type Env =
        { DataContext: DbDataContext
          JwtConfig: JwtConfig
          RefreshTokenExpiresIn: int<minutes>
          HashProvider: HashProvider
          Sha256Provider: Sha256Provider
          KeyValueStorage: IKeyValueStorage
          Logger: ILogger }

    type LogInToken = Env -> Data -> Task<Result>
