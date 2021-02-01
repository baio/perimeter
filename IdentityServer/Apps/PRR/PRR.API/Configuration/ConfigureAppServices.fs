namespace PRR.API.Configuration

open PRR.Domain.Models
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open PRR.API.Infra.Mail.Models
open PRR.API.Infra.Models
open PRR.Domain.Auth
open PRR.API.Common.Configuration

[<AutoOpen>]
module ConfigureServices =

    type AuthConfig =
        { Jwt: JwtConfig
          SSOCookieExpiresIn: int<minutes>
          PerimeterSocialProviders: PerimeterSocialProviders
          ResetPasswordTokenExpiresIn: int<minutes>
          RefreshTokenExpiresIn: int<minutes>
          SignUpTokenExpiresIn: int<minutes>
          Social: SocialConfig }

    type AppConfig =
        { Common: CommonAppConfig
          Auth: AuthConfig
          Infra: InfraConfig
          KeyValueStorage: KeyValueStorageConfig
          MailSender: MailSenderConfig
          SendGridApiKey: string }

    type IConfigProvider =
        abstract GetConfig: (unit -> AppConfig)

    type ConfigProvider(appConfig: AppConfig) =
        interface IConfigProvider with
            member __.GetConfig = fun () -> appConfig

    let configureAppServices (config: AppConfig) (services: IServiceCollection) =

        // common
        configureAuthorization config.Auth.Jwt.AccessTokenSecret services
        configureDataContext config.Common.DataContext services
        configureLogging config.Common.Logging services
        configureTracing config.Common.Tracing services
        configureHealthCheck config.Common.HealthCheck services
        configureConfigProvider config services
        configureServiceBus [] services

        configureInfra config.Infra services
        configureKeyValueStorage config.KeyValueStorage services
        configureSendMail config.SendGridApiKey config.MailSender services

        services.AddSingleton<IConfigProvider>(ConfigProvider config)
        |> ignore
