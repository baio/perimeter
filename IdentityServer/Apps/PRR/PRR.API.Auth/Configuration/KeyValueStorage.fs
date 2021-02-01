namespace PRR.API.Configuration

open DataAvail.KeyValueStorage.Mongo
open DataAvail.KeyValueStorage.Core
open Microsoft.Extensions.DependencyInjection

type KeyValueStorageConfig =
    { ConnectionString: string
      DbName: string
      CollectionName: string }

[<AutoOpen>]
module private KeyValueStorage =

    let configureKeyValueStorage (config: KeyValueStorageConfig) (services: IServiceCollection) =
        let kvStorage =
            KeyValueStorageMongo(config.ConnectionString, config.DbName, config.CollectionName)

        kvStorage.CreateIndexes() |> ignore
        services.AddSingleton<IKeyValueStorage>(kvStorage) |> ignore
