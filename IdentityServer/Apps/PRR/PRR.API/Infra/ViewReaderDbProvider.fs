namespace PRR.API.Infra

open MongoDB.Driver
open PRR.API.Infra.Models

[<AutoOpen>]
module ViewsReaderDbProvider =

    type ViewsReaderDbProvider(connectionString: string) =
        let client = MongoClient(connectionString)

        let db =
            connectionString.Split "/"
            |> Seq.last
            |> client.GetDatabase

        interface IViewsReaderDbProvider with
            member __.ViewsReaderDb = db
