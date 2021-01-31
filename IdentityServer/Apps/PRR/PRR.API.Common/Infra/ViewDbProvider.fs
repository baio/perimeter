namespace PRR.API.Common.Infra

open MongoDB.Driver
open MongoDB.Driver.Core.Extensions.DiagnosticSources

[<AutoOpen>]
module ViewsDbProvider =
    
    type IViewsDbProvider =
        abstract Db: IMongoDatabase
    
    type ViewsDbProvider(connectionString: string, dbName: string) =

        let clientSettings =
            MongoClientSettings.FromUrl(MongoUrl connectionString)

        do
            clientSettings.ClusterConfigurator <-
                (fun cb ->
                    (cb.Subscribe(DiagnosticsActivityEventSubscriber())
                     |> ignore))

        let client = MongoClient(clientSettings)

        let db = dbName |> client.GetDatabase

        interface IViewsDbProvider with
            member __.Db = db
