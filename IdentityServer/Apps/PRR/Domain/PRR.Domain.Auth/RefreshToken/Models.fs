namespace PRR.Domain.Auth.RefreshToken

open Common.Domain.Models
open PRR.Data.DataContext
open PRR.Domain.Auth.SignIn
open PRR.System.Models
open System
open System.Threading.Tasks

[<AutoOpen>]
module Models =

    type Env =
        { DataContext: DbDataContext
          JwtConfig: JwtConfig
          HashProvider: HashProvider }

    type Data =
        { RefreshToken: string }

    type AccessToken = Token

    type RefreshToken = Env -> AccessToken -> RefreshToken.Item -> Task<SignInResult * Events>
