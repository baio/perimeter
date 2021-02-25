namespace PRR.Domain.Tenant

open PRR.Domain.Models
open PRR.Data.DataContext

[<AutoOpen>]
module Models =

    type IAuthStringsGetter =
        { ClientId: unit -> string
          ClientSecret: unit -> string
          AuthorizationCode: unit -> string
          HS256SigningSecret: unit -> string
          RS256XMLParams: unit -> string }

    type AuthConfig =
        { IdTokenExpiresIn: int<minutes>
          AccessTokenExpiresIn: int<minutes>
          RefreshTokenExpiresIn: int<minutes> }

    type Env =
        { DataContext: DbDataContext
          AuthStringsProvider: IAuthStringsGetter
          AuthConfig: AuthConfig }
