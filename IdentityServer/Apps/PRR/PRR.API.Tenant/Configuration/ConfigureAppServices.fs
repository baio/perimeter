namespace PRR.API.Tenant.Configuration

open Microsoft.Extensions.DependencyInjection
open PRR.API.Common.Configuration
open PRR.Domain.Tenant
open PRR.API.Tenant.Infra

type AppConfig =
    { Common: CommonAppConfig
      AccessTokenSecret: string
      TenantAuth: AuthConfig  }

[<AutoOpen>]
module ConfigureAppServices =

    let configureAppServices (config: AppConfig) (services: IServiceCollection) =
        // common
        configureAuthorization config.AccessTokenSecret services
        configureDataContext config.Common.DataContext services
        configureLogging config.Common.Logging services
        configureTracing config.Common.Tracing services
        configureHealthCheck config.Common.HealthCheck services
        configureViewStorage config.Common.ViewStorage services
        configureConfigProvider config services
        configureServiceBus [] services
        // tenant
        services.AddSingleton<IAuthStringsGetterProvider>(AuthStringsProvider())
        
