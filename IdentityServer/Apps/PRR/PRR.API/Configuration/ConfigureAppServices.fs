namespace PRR.API.Configuration

open Common.Domain.Models
open Microsoft.Extensions.DependencyInjection
open PRR.API.Infra.Models
open PRR.Domain.Auth
open PRR.Sys.Models

[<AutoOpen>]
module ConfigureServices =

    type AuthConfig =
        { Jwt: JwtConfig
          SSOCookieExpiresIn: int<minutes>
          PerimeterSocialProviders: PerimeterSocialProviders
          Social: SocialConfig }

    type AppConfig =
        { Logging: LoggingEnv
          Tracing: TracingEnv
          Auth: AuthConfig
          Infra: InfraConfig
          DataContext: DataContextConfig
          Actors: ActorsConfig }

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
        services.AddSingleton<IConfigProvider>(ConfigProvider config)
        |> ignore
