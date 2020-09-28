﻿namespace PRR.System.Views

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
module LogInView =

    type private Doc =
        { _id: ObjectId
          email: string
          dateTime: DateTime
          domainId: int
          appIdentifier: string
          seqNr: int64 }

    let private handleStreamedEvents col seqNr =
        function
        | UserLogInTokenSuccessEvent (_, _, data) ->
            insertOne
                col
                { _id = ObjectId.GenerateNewId()
                  email = data.UserEmail
                  domainId = data.DomainId
                  appIdentifier = data.AppIdentifier
                  dateTime = data.Date
                  seqNr = seqNr }
        | _ -> ()

    let initLoginView (viewDb: IMongoDatabase) sys aref =

        let col = viewDb.GetCollection<Doc>("login_view")

        let keys =
            [| createIndexModel <@ fun x -> x.seqNr @> BsonOrderDesc
               createIndexModel <@ fun x -> x.dateTime @> BsonOrderDesc
               createIndexModel <@ fun x -> x.email @> BsonOrderAsc |]

        createIndexesRange col keys |> ignore

        let seqNrMax = maxBy col (<@ fun x -> x.seqNr @>)

        let readJournal =
            PersistenceQuery.Get(sys).ReadJournalFor<MongoDbReadJournal>(MongoDbReadJournal.Identifier)

        let handler = handleStreamedEvents col

        eventsHandlerByActor sys readJournal aref handler seqNrMax
        |> ignore
