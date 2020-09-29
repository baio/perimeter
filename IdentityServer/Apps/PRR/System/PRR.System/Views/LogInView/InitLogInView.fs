namespace PRR.System.Views.LogInView

open System
open System.Threading.Tasks
open Akka.Persistence.MongoDb.Query
open Akka.Persistence.Query
open FSharp.Akkling.CQRS
open FSharp.MongoDB.Driver

open MongoDB.Bson
open MongoDB.Driver
open PRR.System.Models

[<AutoOpen>]
module InitLogInView =

    let private handleStreamedEvents col seqNr =
        function
        | LogIn.Event evt ->
            match evt with
            | LogIn.CodeRemoved (_, data) ->
                insertOne
                    col
                    { _id = ObjectId.GenerateNewId().ToString()
                      isManagementClient = data.IsManagementClient
                      email = data.UserEmail
                      domainId = data.DomainId
                      appIdentifier = data.AppIdentifier
                      dateTime = data.Date
                      seqNr = seqNr }
            | _ -> ()
        | _ -> ()

    let initLoginView (viewDb: IMongoDatabase) sys aref =

        let col =
            viewDb.GetCollection<Doc>(LOGIN_VIEW_COLLECTION_NAME)

        let keys =
            [| createIndexModel <@ fun x -> x.seqNr @> BsonOrderDesc
               createIndexModel <@ fun x -> x.dateTime @> BsonOrderDesc
               createIndexModel <@ fun x -> x.email @> BsonOrderAsc
               createIndexModel <@ fun x -> x.isManagementClient @> BsonOrderAsc |]

        createIndexesRange col keys |> ignore

        let seqNrMax = maxBy col (<@ fun x -> x.seqNr @>)

        let readJournal =
            PersistenceQuery.Get(sys).ReadJournalFor<MongoDbReadJournal>(MongoDbReadJournal.Identifier)

        let handler = handleStreamedEvents col

        eventsHandlerByActor sys readJournal aref handler seqNrMax
        |> ignore
