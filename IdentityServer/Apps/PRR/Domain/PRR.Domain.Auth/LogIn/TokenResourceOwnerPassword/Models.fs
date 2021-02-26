namespace PRR.Domain.Auth.LogIn.TokenResourceOwnerPassword

open System.Threading.Tasks
open MassTransit

[<AutoOpen>]
module Models =

    open DataAvail.KeyValueStorage.Core
    open Microsoft.Extensions.Logging
    open PRR.Data.DataContext
    open PRR.Domain.Auth
    open PRR.Domain.Models
    open PRR.Domain.Auth.LogIn.Common

    type Data =
        { Grant_Type: string
          Client_Id: string
          Username: string
          Password: string
          Scope: string }

    type Env =
        { DataContext: DbDataContext
          JwtConfig: JwtConfig
          RefreshTokenExpiresIn: int<minutes>
          HashProvider: HashProvider
          StringSalter: StringSalter
          Sha256Provider: Sha256Provider
          KeyValueStorage: IKeyValueStorage
          Logger: ILogger
          PublishEndpoint: IPublishEndpoint }

    type TokenResourceOwnerPassword = Env -> Data -> Task<LogInResult>
