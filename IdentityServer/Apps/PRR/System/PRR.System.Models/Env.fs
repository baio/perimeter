namespace PRR.System.Models

open Common.Domain.Models
open PRR.Data.DataContext
open System
open System.Threading.Tasks

[<AutoOpen>]
module Env =
    type SendMail = SendMailParams -> Task<unit>

    type IDataContextProvider =
        inherit IDisposable
        abstract DataContext: DbDataContext


    type AuthConfig =
        { IdTokenExpiresIn: int<minutes>
          AccessTokenExpiresIn: int<minutes>
          RefreshTokenExpiresIn: int<minutes>
          SignUpTokenExpiresIn: int<minutes>
          ResetPasswordTokenExpiresIn: int<minutes> }

    type GetDataContextProvider = unit -> IDataContextProvider

    type SystemEnv =
        { HashProvider: HashProvider        
          PasswordSalter: StringSalter
          SendMail: SendMail
          GetDataContextProvider: GetDataContextProvider
          AuthConfig: AuthConfig
          // For tests
          EventHandledCallback: Events -> unit }
