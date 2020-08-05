namespace PRR.API.Infra
open Akka.Actor
open Akkling
open PRR.Domain.Auth.SignIn.Models
open Common.Domain.Models
open PRR.System.Models
open PRR.System.Models.Events

[<AutoOpen>]
module Models = 

    type AppConfig =
        {
          SignUpTokenExpiresIn: int<minutes>
          ResetPasswordTokenExpiresIn: int<minutes>
          PasswordSecret: string
          Jwt: JwtConfig }

    type IConfig =
        abstract GetConfig: (unit -> AppConfig) with get

    type IHashProvider =
        abstract GetHash: (unit -> string) with get                

    type IPasswordSaltProvider =
        abstract SaltPassword: (string -> string) with get

    type ISHA256Provider =
        abstract GetSHA256: (string -> string)  with get              

