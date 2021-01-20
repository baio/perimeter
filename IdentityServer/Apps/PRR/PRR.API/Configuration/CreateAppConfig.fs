﻿namespace PRR.API.Configuration

open Common.Domain.Models
open Microsoft.Extensions.Configuration
open PRR.API.Infra.Mail
open PRR.Domain.Auth

[<AutoOpen>]
module CreateAppConfig =

    let createAppConfig envName (configuration: IConfiguration) =

        let jwt =
            { IdTokenSecret = configuration.GetValue<string>("Auth:Jwt:AccessTokenSecret")
              AccessTokenSecret = configuration.GetValue<string>("Auth:Jwt:AccessTokenSecret")
              IdTokenExpiresIn = configuration.GetValue<int<minutes>>("Auth:Jwt:IdTokenExpiresInMinutes")
              AccessTokenExpiresIn = configuration.GetValue<int<minutes>>("Auth:Jwt:IdTokenExpiresInMinutes")
              RefreshTokenExpiresIn = configuration.GetValue<int<minutes>>("Auth:Jwt:RefreshTokenExpiresInMinutes")
              CodeExpiresIn = configuration.GetValue<int<minutes>>("Auth:Jwt:CodeExpiresInMinutes") }

        let mailEnv: MailEnv =
            { FromEmail = configuration.GetValue("MailSender:FromEmail")
              FromName = configuration.GetValue("MailSender:FromName")
              Project =
                  { Name = configuration.GetValue("MailSender:Project:Name")
                    BaseUrl = configuration.GetValue("MailSender:Project:BaseUrl")
                    ConfirmSignUpUrl = configuration.GetValue("MailSender:Project:ConfirmSignUpUrl")
                    ResetPasswordUrl = configuration.GetValue("MailSender:Project:ResetPasswordUrl") } }

        let sendGridApiKey = configuration.GetValue("SendGridApiKey")

        let connectionStrings: ActorsConnectionsString =
            { MongoJournal = configuration.GetConnectionString("MongoJournal")
              MongoSnapshot = configuration.GetConnectionString("MongoSnapshot")
              MongoViews = configuration.GetConnectionString("MongoViews") }

        let actorsConfig: ActorsConfig =
            { Mail = mailEnv
              SendGridApiKey = sendGridApiKey
              Jwt = jwt
              ConnectionStrings = connectionStrings
              SignUpTokenExpiresIn = configuration.GetValue<int<minutes>>("Auth:SignUpTokenExpiresInMinutes")
              ResetPasswordTokenExpiresIn =
                  configuration.GetValue<int<minutes>>("Auth:ResetPasswordTokenExpiresInMinutes")
              EnvironmentName = envName }

        let authConfig: AuthConfig =
            { Jwt = jwt
              SSOCookieExpiresIn = configuration.GetValue<int<minutes>>("Auth:SSOCookieExpiresInMinutes")
              PerimeterSocialProviders =
                  { Github =
                        { ClientId = configuration.GetValue<string>("Auth:PerimeterSocialProviders:Github:ClientId")
                          SecretKey = configuration.GetValue<string>("Auth:PerimeterSocialProviders:Github:SecretKey") }
                    Google =
                        { ClientId = configuration.GetValue<string>("Auth:PerimeterSocialProviders:Google:ClientId")
                          SecretKey = configuration.GetValue<string>("Auth:PerimeterSocialProviders:Google:SecretKey") }
                    Twitter =
                        { ClientId = configuration.GetValue<string>("Auth:PerimeterSocialProviders:Twitter:ClientId")
                          SecretKey = configuration.GetValue<string>("Auth:PerimeterSocialProviders:Twitter:SecretKey") } }
              Social =
                  { CallbackUrl = configuration.GetValue<string>("Auth:Social:CallbackUrl")
                    CallbackExpiresIn =
                        configuration.GetValue<int<milliseconds>>("Auth:Social:CallbackExpiresInMilliseconds") } }

        let ignoreObserveApiPaths = [ "/metrics" ]

        { Actors = actorsConfig
          DataContext = { ConnectionString = configuration.GetConnectionString "PostgreSQL" }
          Infra =
              { MongoViewsConnectionString = configuration.GetConnectionString("MongoViews")
                PasswordSecret = configuration.GetValue<string>("Auth:PasswordSecret") }
          Auth = authConfig
          Logging =
              { Config = { ServiceUrl = configuration.GetValue("Logging:Seq:ServiceUrl") }
                IgnoreApiPaths = ignoreObserveApiPaths }
          Tracing =
              { Config =
                    { ServiceName = configuration.GetValue("Tracing:Jaeger:ServiceName")
                      AgentHost = configuration.GetValue("Tracing:Jaeger:AgentHost")
                      AgentPort = configuration.GetValue<int>("Tracing:Jaeger:AgentPort") }
                IgnoreApiPaths = ignoreObserveApiPaths } }