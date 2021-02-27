namespace PRR.Domain.Auth.LogIn.TokenClientCredentials

[<AutoOpen>]
module Models =

    open System.Threading.Tasks
    open MassTransit
    open DataAvail.KeyValueStorage.Core
    open Microsoft.Extensions.Logging
    open PRR.Data.DataContext
    open PRR.Domain.Auth
    open PRR.Domain.Models
    open PRR.Domain.Auth.LogIn.Common

    type Result = {
        access_token: string
        token_type: string
        expires_in: int
    }
    
    type Data =
        { Grant_Type: string
          Client_Id: string
          Client_Secret: string
          Audience: string }

    type Env =
        { DataContext: DbDataContext
          JwtConfig: JwtConfig
          HashProvider: HashProvider
          Sha256Provider: Sha256Provider
          Logger: ILogger
          PublishEndpoint: IPublishEndpoint }

    type TokenClientCredentials = Env -> Data -> Task<Result>
