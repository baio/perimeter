namespace PRR.System

open Akka.Actor
open Akkling
open Common.Domain.Models
open Common.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.System.Models
open System
open System.Threading.Tasks

// TODO : This module are not related to PRR.System domain and should be moved down level

[<AutoOpen>]
module Utils =

    let private taskWithTimeout (timeout: int<milliseconds>) (t: Task<'a>) =
        let tm =
            task {
                do! Task.Delay(int timeout)
                return! raiseTask (TimeoutException "Timeout expired")
            }
        task {
            let! r = Task.WhenAny(t, tm)
            return! r }

    let actorOfTask (timeout: int<milliseconds>) (sendTo: IActorRef<'b>) (t: 'a -> Task<'b>) f =
        (fun (ctx: Actor<_>) ->
        let rec loop() =
            actor {
                let! data = ctx.Receive()
                task {
                    try
                        let! t' = data
                                  |> t
                                  |> taskWithTimeout timeout
                        return t'
                    with e ->
                        // TODO : Supervisors doesn't catch this ?
                        return f (e)
                }
                |> Async.AwaitTask
                |!> sendTo
            }
        loop())

    let commandActorOfTask (timeout: int<milliseconds>) (sendTo: IActorRef<Events>) (t: _ -> Task<Events>) =
        actorOfTask timeout sendTo t CommandFailureEvent

    /// Given actor `sendTo` with ask - response pattern ( actor <! msg (data, tellTo) )
    /// Will create transient actor `tellTo` and then ask `sendTo` with `msg` returned from msgFactory (tellTo) -> Query(data, tellTo)
    /// Then will wait `timeout` to receive response to `tellTo` and set one as Task result
    /// Task itself will be returned as function result
    /// IActorRefFactory required to create transient actor
    let taskOfAskResponseActor (timeout: int<milliseconds>) (sys: IActorRefFactory) (sendTo: IActorRef<'b>)
        (msgFactory: IActorRef<'a> -> 'b) =
        let t = TaskCompletionSource<'a>()

        let a =
            spawnAnonymous sys <| (props
                                       (actorOf (fun x ->
                                           t.TrySetResult(x) |> ignore
                                           Stop)))

        let msg = msgFactory a
        sendTo <! msg
        task {
            try
                return! t.Task |> taskWithTimeout timeout
            with e ->
                (retype a) <! PoisonPill.Instance
                return! raiseTask e
        }

    let taskOfQueryActor timeout sys sendTo msgFactory (eventsRef: IActorRef<Events>) =
        task {
            try
                return! taskOfAskResponseActor timeout sys sendTo msgFactory
            with e ->
                eventsRef <! (QueryFailureEvent e)
                return! raiseTask e
        }
