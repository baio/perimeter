namespace PRR.API.Infra

open MongoDB.Driver
open PRR.API.Infra.Models

[<AutoOpen>]
module ViewsDbProvider =

    type ViewsDbProvider(connectionString: string, dbName: string) =
        let client = MongoClient(connectionString)

        let db = dbName |> client.GetDatabase

        interface IViewsDbProvider with
            member __.Db = db
