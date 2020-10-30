namespace PRR.Domain.Auth

open Common.Domain.Models

[<AutoOpen>]
module JwtConfig =
    type JwtConfig =
        { IdTokenSecret: string
          AccessTokenSecret: string
          IdTokenExpiresIn: int<minutes>
          AccessTokenExpiresIn: int<minutes>
          RefreshTokenExpiresIn: int<minutes>
          CodeExpiresIn: int<minutes> }
