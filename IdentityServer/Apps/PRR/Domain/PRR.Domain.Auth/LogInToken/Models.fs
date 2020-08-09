namespace PRR.Domain.Auth.LogInToken

open Common.Domain.Models

open PRR.Data.DataContext
open PRR.Domain.Auth
open PRR.System.Models
open System.Threading.Tasks

[<AutoOpen>]
module Models =
    type Data =
        { Grant_Type: string
          Code: Token
          Redirect_Uri: string
          Client_Id: string
          Code_Verifier: string }

    type Result =
        { IdToken: string
          AccessToken: string
          RefreshToken: string }

    type Env =
        { DataContext: DbDataContext
          JwtConfig: JwtConfig
          HashProvider: HashProvider
          Sha256Provider: Sha256Provider }

    type LogInToken = Env -> LogIn.Item -> Data -> Task<Result * Events>
