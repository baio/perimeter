namespace PRR.API.Infra

open Common.Domain.Models
open PRR.Domain.Auth

[<AutoOpen>]
module Models =

    type AppConfig =
        { SignUpTokenExpiresIn: int<minutes>
          ResetPasswordTokenExpiresIn: int<minutes>
          SSOCookieExpiresIn: int<minutes>
          PasswordSecret: string
          Jwt: JwtConfig }

    type IConfig =
        abstract GetConfig: (unit -> AppConfig) with get

    type IHashProvider =
        abstract GetHash: (unit -> string) with get

    type IPasswordSaltProvider =
        abstract SaltPassword: (string -> string) with get

    type ISHA256Provider =
        abstract GetSHA256: (string -> string) with get

    type IAuthStringsProvider =
        abstract AuthStringsProvider: AuthStringsProvider
