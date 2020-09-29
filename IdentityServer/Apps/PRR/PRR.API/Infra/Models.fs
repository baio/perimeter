namespace PRR.API.Infra

open Common.Domain.Models
open MongoDB.Driver
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
        abstract GetConfig: (unit -> AppConfig)

    type IHashProvider =
        abstract GetHash: (unit -> string)

    type IPasswordSaltProvider =
        abstract SaltPassword: (string -> string)

    type ISHA256Provider =
        abstract GetSHA256: (string -> string)

    type IAuthStringsProvider =
        abstract AuthStringsProvider: AuthStringsProvider

    type IViewsReaderDbProvider =
        abstract ViewsReaderDb: IMongoDatabase
