namespace PRR.API.Auth.Configuration

open PRR.Domain.Models
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open PRR.API.Auth.Infra.Mail.Models
open PRR.API.Auth.Infra.Models
open PRR.Domain.Auth
open PRR.API.Common.Configuration

[<AutoOpen>]
module ConfigureServices =

    type AuthConfig =
        { Jwt: JwtConfig
          LoginPageDomain: string
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
          SendMailConfig: SendMailConfig }

    type IConfigProvider =
        abstract GetConfig: (unit -> AppConfig)

    type ConfigProvider(appConfig: AppConfig) =
        interface IConfigProvider with
            member __.GetConfig = fun () -> appConfig

    let configureAppServices (config: AppConfig) (services: IServiceCollection) =

        printfn "Start app with config %O" config
        // common
        configureLogging config.Common.Logging services
        configureAuthorization config.Auth.Jwt.AccessTokenSecret services
        configureDataContext config.Common.DataContext services
        configureTracing config.Common.Tracing services
        configureConfigProvider config services
        configureHealthCheck config.Common.HealthCheck services
        configureServiceBus config.Common.ServiceBus [] services
        configureInfra config.Infra services
        configureKeyValueStorage config.KeyValueStorage services

        configureSendMail config.SendMailConfig config.MailSender services

        services.AddSingleton<IConfigProvider>(ConfigProvider config)
        |> ignore
