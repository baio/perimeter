namespace PRR.API.Tenant.Configuration

open Microsoft.Extensions.DependencyInjection
open PRR.API.Common.Configuration
open PRR.Domain.Tenant
open PRR.API.Tenant.Infra
open PRR.API.Tenant.EventHandlers

open MassTransit
open MassTransit.RabbitMqTransport

type AppConfig =
    { Common: CommonAppConfig
      AccessTokenSecret: string
      TenantAuth: AuthConfig }


[<AutoOpen>]
module ConfigureAppServices =


    let configureAppServices (config: AppConfig) (services: IServiceCollection) =

        printfn "Start app with config %O" config
        // common
        configureAuthorization config.AccessTokenSecret services
        configureDataContext config.Common.DataContext services
        configureLogging config.Common.Logging services
        configureTracing config.Common.Tracing services
        configureHealthCheck config.Common.HealthCheck services

        let viewDbProvider =
            configureViewStorage config.Common.ViewStorage services

        configureConfigProvider config services
        configureServiceBus config.Common.ServiceBus [ typeof<LogInEventHandler> ] services
        // tenant
        services.AddSingleton<IAuthStringsGetterProvider>(AuthStringsProvider(authStringsGetter))
        |> ignore
        // event handlers
        let viewDb = viewDbProvider.Db
        initEventHandlers viewDb
