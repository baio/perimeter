namespace PRR.Domain.Tenant

open Common.Domain.Models
open PRR.Data.DataContext

[<AutoOpen>]
module Models =

    type AuthConfig =
        { IdTokenExpiresIn: int<minutes>
          AccessTokenSecret: string
          AccessTokenExpiresIn: int<minutes>
          RefreshTokenExpiresIn: int<minutes> }

    type Env =
        { DataContext: DbDataContext
          AuthStringsProvider: AuthStringsGetter
          AuthConfig: AuthConfig }
