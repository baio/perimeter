namespace PRR.System.Views

open MongoDB.Driver

[<AutoOpen>]
module SetUpViewDb =

    let setUpViewDb (connectionString: string) =
        let mongo = MongoClient(connectionString)
        let dbName = 
            connectionString.Split "?"
            |> Seq.head
            |> (fun x -> x.Split "/")
            |> Seq.last
        mongo.GetDatabase dbName
