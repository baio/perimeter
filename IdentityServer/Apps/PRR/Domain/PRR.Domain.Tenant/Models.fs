namespace PRR.Domain.Tenant

open Common.Domain.Models
open PRR.Data.DataContext

[<AutoOpen>]
module Models =

    type AuthConfig =
        { IdTokenExpiresIn: int<minutes>
          AccessTokenExpiresIn: int<minutes>
          RefreshTokenExpiresIn: int<minutes> }

    type Env =
        { DataContext: DbDataContext
          AuthStringsProvider: AuthStringsProvider
          AuthConfig: AuthConfig }
