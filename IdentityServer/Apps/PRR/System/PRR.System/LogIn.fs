namespace PRR.System

open Akka.Actor
open Akka.Persistence
open Akkling
open Akkling.Persistence
open Common.Domain.Models
open PRR.System.Models
open PRR.System.Models.LogIn
open System

[<AutoOpen>]
module private LogIn =

    type private State = Map<Token, Item>

    let logIn (events: IActorRef<Models.Events.Events>) =
        propsPersist (fun ctx ->
            let rec loop (state: State) =
                actor {
                    let! msg = ctx.Receive()
                    match msg with
                    | Command cmd ->
                        match cmd with
                        // TODO : Remove expired tokens
                        // TODO : Restart actor from test itself
                        | Restart ->
                            raise (exn "Test Restart")
                            return! loop state
                        | AddCode(item) ->
                            return Persist(Event(CodeAdded(item)))
                        | RemoveCode(code) ->
                            return Persist(Event(CodeRemoved(code)))
                        | MakeSnapshot ->
                            typed ctx.SnapshotStore
                            <! SaveSnapshot(SnapshotMetadata(ctx.Pid, ctx.LastSequenceNr()), state)
                            return! loop state
                    | Event evt ->
                        match evt with
                        | CodeAdded(item) as evt' ->
                            let state = state.Add(item.Code, item)
                            if isNotRecovering ctx then events <! (LogInEvent evt')
                            return! loop state
                        | CodeRemoved(code) as evt' ->
                            let state = state.Remove code
                            if isNotRecovering ctx then events <! (LogInEvent evt')
                            return! loop state
                    | Query q ->
                        match q with
                        | GetCode(code, sendTo) ->
                            let result =
                                state
                                |> Map.tryFind code
                                |> valueResultFromOption
                            sendTo <! result
                            return! loop state
                    | SnapshotOffer snapshot ->
                        return! loop snapshot
                }
            loop Map.empty)
