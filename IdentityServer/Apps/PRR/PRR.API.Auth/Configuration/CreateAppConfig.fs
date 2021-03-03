namespace PRR.API.Auth.Configuration

open PRR.Domain.Models
open Microsoft.Extensions.Configuration
open PRR.API.Auth.Infra.Mail
open PRR.Domain.Auth
open PRR.API.Common.Configuration

[<AutoOpen>]
module CreateAppConfig =

    let createAppConfig (configuration: IConfiguration) =

        let common = createCommonAppConfig configuration

        let jwt =
            { IdTokenSecret = configuration.GetValue<string>("Auth:Jwt:IdTokenSecret")
              AccessTokenSecret = configuration.GetValue<string>("Auth:Jwt:AccessTokenSecret")
              IdTokenExpiresIn = configuration.GetValue<int<minutes>>("Auth:Jwt:IdTokenExpiresInMinutes")
              AccessTokenExpiresIn = configuration.GetValue<int<minutes>>("Auth:Jwt:IdTokenExpiresInMinutes")
              RefreshTokenExpiresIn = configuration.GetValue<int<minutes>>("Auth:Jwt:RefreshTokenExpiresInMinutes")
              CodeExpiresIn = configuration.GetValue<int<minutes>>("Auth:Jwt:CodeExpiresInMinutes") }

        let mailSenderConfig: MailSenderConfig =
            { FromEmail = configuration.GetValue("MailSender:FromEmail")
              FromName = configuration.GetValue("MailSender:FromName")
              Project =
                  { Name = configuration.GetValue("MailSender:Project:Name")
                    BaseUrl = configuration.GetValue("MailSender:Project:BaseUrl")
                    ConfirmSignUpUrl = configuration.GetValue("MailSender:Project:ConfirmSignUpUrl")
                    ResetPasswordUrl = configuration.GetValue("MailSender:Project:ResetPasswordUrl") } }

        let authConfig: AuthConfig =
            { Jwt = jwt
              LoginPageDomain = configuration.GetValue("Auth:LoginPageDomain")
              RefreshTokenExpiresIn = configuration.GetValue<int<minutes>>("Auth:Jwt:RefreshTokenExpiresInMinutes")
              SignUpTokenExpiresIn = configuration.GetValue<int<minutes>>("Auth:SignUpTokenExpiresInMinutes")
              ResetPasswordTokenExpiresIn =
                  configuration.GetValue<int<minutes>>("Auth:ResetPasswordTokenExpiresInMinutes")
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

        let keyValueStorageConfig: KeyValueStorageConfig =
            { ConnectionString = configuration.GetValue("MongoKeyValueStorage:ConnectionString")
              DbName = configuration.GetValue("MongoKeyValueStorage:DbName")
              CollectionName = configuration.GetValue("MongoKeyValueStorage:CollectionName") }

        let sendMailConfig: SendMailConfig =
            { DomainName = configuration.GetValue("MailGun:DomainName")
              ApiKey = configuration.GetValue("MailGun:ApiKey")
              Region = configuration.GetValue("MailGun:Region") }

        { Common = common
          SendMailConfig = sendMailConfig
          MailSender = mailSenderConfig
          KeyValueStorage = keyValueStorageConfig
          Infra = { PasswordSecret = configuration.GetValue<string>("Auth:PasswordSecret") }
          Auth = authConfig }
