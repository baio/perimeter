namespace PRR.API.Common.Configuration

open Microsoft.Extensions.DependencyInjection
open PRR.API.Common.Infra

type ViewStorageConfig =
    { ConnectionString: string
      DbName: string }

[<AutoOpen>]
module ViewsStorage =

    let configureViewStorage (config: ViewStorageConfig) (services: IServiceCollection) =

        let dbProvider =
            ViewsDbProvider(config.ConnectionString, config.DbName) :> IViewsDbProvider

        services.AddSingleton<IViewsDbProvider> dbProvider
        ()
