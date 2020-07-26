namespace PRR.API.Infra

open Microsoft.Extensions.Configuration
open Common.Domain.Models

[<AutoOpen>]
module Config = 

    open Models

    let getConfig (configuration: IConfiguration) () =
        {
            SignUpTokenExpiresIn =
                configuration.GetValue<int<minutes>>("Auth:SignUpTokenExpiresInMinutes")
            ResetPasswordTokenExpiresIn =
                configuration.GetValue<int<minutes>>("Auth:ResetPasswordTokenExpiresInMinutes")
            PasswordSecret = 
                configuration.GetValue<string>("Auth:PasswordSecret")
            Jwt = {
                IdTokenSecret = configuration.GetValue<string>("Auth:Jwt:AccessTokenSecret")
                AccessTokenSecret = configuration.GetValue<string>("Auth:Jwt:AccessTokenSecret")
                IdTokenExpiresIn = configuration.GetValue<int<minutes>>("Auth:Jwt:IdTokenExpiresInMinutes")
                AccessTokenExpiresIn = configuration.GetValue<int<minutes>>("Auth:Jwt:IdTokenExpiresInMinutes")
                RefreshTokenExpiresIn = configuration.GetValue<int<minutes>>("Auth:Jwt:RefreshTokenExpiresInMinutes")
            }
        }
        
    type Config(configuration: IConfiguration) = 
        interface IConfig with
            member __.GetConfig = (getConfig configuration)

