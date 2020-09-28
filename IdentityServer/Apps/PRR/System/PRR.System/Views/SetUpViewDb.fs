namespace PRR.System.Views

open MongoDB.Driver

[<AutoOpen>]
module SetUpViewDb =

    let setUpViewDb (connectionString: string) =
        let mongo = MongoClient(connectionString)
        connectionString.Split "/"
        |> Seq.last
        |> mongo.GetDatabase
    