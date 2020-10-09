namespace FSharp.Akkling.CQRS

open Akka.Actor
open Akkling
open Akka.Persistence.Query
open Akka.Streams

[<AutoOpen>]
module JournalHelpers =

    let handleEnvelope<'evt> handleEvent (eventEnvelope: EventEnvelope) =
        match eventEnvelope.Event with
        | :? 'evt as evt -> handleEvent eventEnvelope.SequenceNr evt
        | _ -> ()

    let eventsHandlerByActor (ctx: IActorRefFactory)
                             (readJournal: IEventsByPersistenceIdQuery)
                             (actorRef: IActorRef<'a>)
                             handleEvent
                             lastSequenceNr
                             =

        let events =
            readJournal.EventsByPersistenceId(actorRef.Path.Name, lastSequenceNr + 1L, 1000L)

        // materialize stream, consuming events
        let mat = ActorMaterializer.Create ctx

        events.RunForeach((fun envelope -> handleEnvelope<'a> handleEvent envelope), mat)
