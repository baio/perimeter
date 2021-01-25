namespace PRR.API.Configuration

open Common.Domain.Models
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Logging
open PRR.API.Infra.Mail.Models
open PRR.API.Infra.Models
open PRR.Domain.Auth
open PRR.Sys.Models

[<AutoOpen>]
module ConfigureServices =

    type AuthConfig =
        { Jwt: JwtConfig
          SSOCookieExpiresIn: int<minutes>
          PerimeterSocialProviders: PerimeterSocialProviders
          ResetPasswordTokenExpiresIn: int<minutes>
          Social: SocialConfig }

    type AppConfig =
        { Logging: LoggingEnv
          Tracing: TracingEnv
          Auth: AuthConfig
          Infra: InfraConfig
          DataContext: DataContextConfig
          Actors: ActorsConfig
          HealthCheck: HealthCheckConfig
          KeyValueStorage: KeyValueStorageConfig
          MailSender: MailSenderConfig
          SendGridApiKey: string }

    type IConfigProvider =
        abstract GetConfig: (unit -> AppConfig)

    type ConfigProvider(appConfig: AppConfig) =
        interface IConfigProvider with
            member __.GetConfig = fun () -> appConfig

    let configureAppServices (config: AppConfig) (services: IServiceCollection) =

        configureAuthorization config.Auth.Jwt services
        configureDataContext config.DataContext services
        configureInfra config.Infra services
        configureLogging config.Logging services
        configureTracing config.Tracing services
        configureActors config.Actors services
        configureHealthCheck config.HealthCheck services
        configureKeyValueStorage config.KeyValueStorage services
        configureSendMail config.SendGridApiKey config.MailSender services
        services.AddSingleton<IConfigProvider>(ConfigProvider config)
        |> ignore
