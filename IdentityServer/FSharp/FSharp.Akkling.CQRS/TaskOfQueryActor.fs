namespace FSharp.Akkling.CQRS

open System
open System.Threading.Tasks
open Akka.Actor
open Akkling
open FSharp.Control.Tasks.V2.ContextInsensitive

[<AutoOpen>]
module TaskOfQueryActor =

    /// Given actor `sendTo` with ask evt
    /// Will create transient actor `tellTo` and then ask `sendTo` with `msg`
    /// where `msg` will be created from `msgFactory` provided with `sendTo`
    /// `tellTo` actor must expect message `Msg IActorRef<'a>`
    /// and send response to the provided in message actor
    /// (typesafe ask tell pattern for akka)
    /// Then will wait `timeout` to receive response to `tellTo` and set one as Task result
    /// Task itself will be returned as function result
    let taskOfQueryActor<'a, 'b> (timeout: int)
                                 (sys: IActorRefFactory)
                                 (sendTo: IActorRef<'b>)
                                 (msgFactory: IActorRef<'a> -> 'b)
                                 : Task<'a> =
        let t = TaskCompletionSource<'a>()

        // actor will send message to target actor and then wait async result
        let actor =
            spawnAnonymous sys
            <| (props
                    (actorOf (fun x ->
                        t.TrySetResult(x) |> ignore
                        Stop)))

        let msg = msgFactory actor
        sendTo <! msg

        task {
            try
                // can't receive result in timeout, return timeout error
                return! (t.Task |> taskWithTimeout timeout)
            with e ->
                // stop actor
                (retype actor) <! PoisonPill.Instance

                return raise e
        }
