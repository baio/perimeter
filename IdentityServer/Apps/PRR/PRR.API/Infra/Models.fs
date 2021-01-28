namespace PRR.API.Infra

open Common.Domain.Models
open MongoDB.Driver

[<AutoOpen>]
module Models =

    type SocialConfig =
        { CallbackUrl: string
          CallbackExpiresIn: int<milliseconds> }

    type IHashProvider =
        abstract GetHash: (unit -> string)

    type IPasswordSaltProvider =
        abstract SaltPassword: (string -> string)

    type ISHA256Provider =
        abstract GetSHA256: (string -> string)

    type IAuthStringsProvider =
        abstract AuthStringsGetter: AuthStringsGetter

    type IViewsDbProvider =
        abstract Db: IMongoDatabase


    type IHttpRequestFunProvider =
        abstract HttpRequestFun: HttpRequestFun


   