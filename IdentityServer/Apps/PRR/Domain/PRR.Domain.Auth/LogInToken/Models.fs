namespace PRR.Domain.Auth.LogInToken

open Common.Domain.Models

open PRR.Data.DataContext
open PRR.Domain.Auth.SignIn
open PRR.System.Models
open System.Threading.Tasks

[<AutoOpen>]
module Models =

    type Data =
        { GrantType: string
          Code: Token
          RedirectUri: string
          ClientId: string
          CodeVerifier: string }

    type Result =
        { IdToken: string
          AccessToken: string
          RefreshToken: string }

    type Env =
        { DataContext: DbDataContext
          JwtConfig: JwtConfig
          HashProvider: HashProvider }

    type LogInToken = Env -> LogIn.Item -> Data -> Task<Result * Events>
