namespace PRR.Domain.Auth.SignIn

open Common.Domain.Models

open PRR.Data.DataContext
open PRR.System.Models
open System.Threading.Tasks

[<AutoOpen>]
module Models =

    type JwtConfig =
        { IdTokenSecret: string
          AccessTokenSecret: string
          IdTokenExpiresIn: int<minutes>
          AccessTokenExpiresIn: int<minutes>
          RefreshTokenExpiresIn: int<minutes> }

    type SignInData =
        { ClientId: ClientId
          // RedirectUri: Uri
          // Scopes: Scope seq    
          Email: string
          Password: string }

    type SignInResult =
        { IdToken: string
          AccessToken: string
          RefreshToken: string }

    type Env =
        { DataContext: DbDataContext
          PasswordSalter: StringSalter
          JwtConfig: JwtConfig
          HashProvider: HashProvider }

    type SignIn = Env -> SignInData -> Task<SignInResult * Events>

    // Login with user's tenant management app client id
    type LogInData =
        { Email: string
          Password: string }
        
    type LogIn = Env -> LogInData -> Task<SignInResult * Events>        
