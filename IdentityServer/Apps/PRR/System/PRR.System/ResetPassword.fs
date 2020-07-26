namespace PRR.System

open Akka.Actor
open Akka.Persistence
open Akkling
open Akkling.Persistence
open Common.Domain.Models
open PRR.System.Models
open PRR.System.Models.ResetPassword
open System

[<AutoOpen>]
module private ResetPassword =

    type private State = Map<Token, Item>

    type Env =
        { HashProvider: HashProvider
          TokenExpiresIn: int<minutes> }

    let resetPassword (env: Env) (events: IActorRef<Models.Events.Events>) =
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
                        | AddToken(email) ->
                            printf "2222 %s" email
                            let item =
                                { Email = email
                                  Token = env.HashProvider()
                                  ExpiredAt = DateTime.UtcNow.AddMinutes(float (int env.TokenExpiresIn)) }
                            return Persist(Event(TokenAdded item))
                        | RemoveTokensWithEmail(email) ->
                            // TODO : Async
                            let tokens =
                                state
                                |> Map.toSeq
                                |> Seq.map snd
                                |> Seq.filter (fun f -> f.Email = email)
                                |> Seq.map (fun x -> x.Token)
                            return PersistAll(tokens |> Seq.map (TokenRemoved >> Event))
                        | MakeSnapshot ->
                            typed ctx.SnapshotStore
                            <! SaveSnapshot(SnapshotMetadata(ctx.Pid, ctx.LastSequenceNr()), state)
                            return! loop state
                    | Event evt ->
                        match evt with
                        | TokenAdded(x) as evt' ->
                            let state = state.Add(x.Token, x)
                            if isNotRecovering ctx then events <! (ResetPasswordEvent evt')
                            return! loop state
                        | TokenRemoved(token) as evt' ->
                            let state = state.Remove(token)
                            if isNotRecovering ctx then events <! (ResetPasswordEvent evt')
                            return! loop state
                    | Query q ->
                        match q with
                        | GetToken(token, sendTo) ->
                            let result =
                                state
                                |> Map.tryFind token
                                |> valueResultFromOption
                            sendTo <! result
                            return! loop state
                    | SnapshotOffer snapshot ->
                        return! loop snapshot
                }
            loop Map.empty)
