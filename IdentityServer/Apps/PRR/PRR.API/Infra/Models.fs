namespace PRR.API.Infra

open Common.Domain.Models
open MongoDB.Driver
open PRR.Domain.Auth
open PRR.Sys.SetUp

[<AutoOpen>]
module Models =

    type SocialConfig =
        { CallbackUrl: string
          CallbackExpiresIn: int<milliseconds> }

    type AppConfig =
        { SignUpTokenExpiresIn: int<minutes>
          ResetPasswordTokenExpiresIn: int<minutes>
          SSOCookieExpiresIn: int<minutes>
          PasswordSecret: string
          Social: SocialConfig
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

    type ISystemActorsProvider =
        abstract SystemActors: SystemActors

    type IHttpRequestFunProvider =
        abstract HttpRequestFun: HttpRequestFun
