namespace PRR.System

open Akka.Actor
open Akka.Persistence
open Akkling
open Akkling.Persistence
open Common.Domain.Models
open PRR.System.Models
open PRR.System.Models.RefreshToken
open System

[<AutoOpen>]
module private RefreshToken =
    type private State = Map<Token, Item>

    let refreshToken (tokenExpiresIs: int<minutes>) =
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
                        | AddToken (item) -> return Persist(Event(TokenAdded item))
                        | RemoveToken (userId) ->
                            let token =
                                state
                                |> Seq.tryFind (fun f -> f.Value.UserId = userId)

                            match token with
                            | Some x -> return Persist(Event(TokenRemoved x.Key))
                            | None -> return! loop state
                        | UpdateToken (x) ->
                            let item =
                                { ClientId = x.ClientId
                                  UserId = x.UserId
                                  Token = x.RefreshToken
                                  ExpiresAt = DateTime.UtcNow.AddMinutes(float (int tokenExpiresIs))
                                  SigningAudience = x.SigningAudience
                                  Scopes = x.Scopes }

                            return Persist(Event(TokenUpdated(item, x.OldRefreshToken)))
                        | MakeSnapshot ->
                            typed ctx.SnapshotStore
                            <! SaveSnapshot(SnapshotMetadata(ctx.Pid, ctx.LastSequenceNr()), state)
                            return! loop state
                    | Event evt ->
                        match evt with
                        | TokenAdded (item) ->
                            let state = state.Add(item.Token, item)
                            return! loop state
                        | TokenUpdated (item, oldToken) ->
                            let state =
                                state.Remove(oldToken).Add(item.Token, item)

                            return! loop state
                        | TokenRemoved (token) ->
                            let state = state.Remove(token)
                            return! loop state
                    | Query q ->
                        match q with
                        | RefreshToken.GetToken (token, sendTo) ->
                            let result =
                                state
                                |> Map.tryFind token
                                |> valueResultFromOption

                            sendTo <! result
                            return! loop state
                    | SnapshotOffer snapshot -> return! loop snapshot
                }

            loop Map.empty)
