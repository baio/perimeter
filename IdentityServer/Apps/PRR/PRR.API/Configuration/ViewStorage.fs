namespace PRR.API.Configuration

open DataAvail.KeyValueStorage.Mongo
open DataAvail.KeyValueStorage.Core
open Microsoft.Extensions.DependencyInjection
open MongoDB.Driver
open PRR.API.EventHandlers
open PRR.API.Infra
open PRR.API.Infra.Models

type ViewStorageConfig =
    { ConnectionString: string
      DbName: string }

[<AutoOpen>]
module private ViewsStorage =

    let configureViewStorage (config: ViewStorageConfig) (services: IServiceCollection) =

        let dbProvider =
            ViewsDbProvider(config.ConnectionString, config.DbName) :> IViewsDbProvider

        services.AddSingleton<IViewsDbProvider> dbProvider
        initEventHandlers dbProvider.Db
        ()
