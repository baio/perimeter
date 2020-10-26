namespace PRR.API.Infra

open Common.Domain.Models
open Microsoft.Extensions.Configuration

[<AutoOpen>]
module Config =

    open Models

    let getConfig (configuration: IConfiguration) () =
        { SignUpTokenExpiresIn = configuration.GetValue<int<minutes>>("Auth:SignUpTokenExpiresInMinutes")
          ResetPasswordTokenExpiresIn = configuration.GetValue<int<minutes>>("Auth:ResetPasswordTokenExpiresInMinutes")
          PasswordSecret = configuration.GetValue<string>("Auth:PasswordSecret")
          SSOCookieExpiresIn = configuration.GetValue<int<minutes>>("Auth:SSOCookieExpiresInMinutes")
          Social =
              { CallbackUrl = configuration.GetValue<string>("Auth:Social:CallbackUrl")
                CallbackExpiresIn =
                    configuration.GetValue<int<milliseconds>>("Auth:Social:CallbackExpiresInMilliseconds") }
          Jwt =
              { IdTokenSecret = configuration.GetValue<string>("Auth:Jwt:AccessTokenSecret")
                AccessTokenSecret = configuration.GetValue<string>("Auth:Jwt:AccessTokenSecret")
                IdTokenExpiresIn = configuration.GetValue<int<minutes>>("Auth:Jwt:IdTokenExpiresInMinutes")
                AccessTokenExpiresIn = configuration.GetValue<int<minutes>>("Auth:Jwt:IdTokenExpiresInMinutes")
                RefreshTokenExpiresIn = configuration.GetValue<int<minutes>>("Auth:Jwt:RefreshTokenExpiresInMinutes")
                CodeExpiresIn = configuration.GetValue<int<minutes>>("Auth:Jwt:CodeExpiresInMinutes") } }

    type Config(configuration: IConfiguration) =
        interface IConfig with
            member __.GetConfig = (getConfig configuration)
