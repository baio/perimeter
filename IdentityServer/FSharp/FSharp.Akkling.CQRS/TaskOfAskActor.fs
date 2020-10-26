namespace FSharp.Akkling.CQRS

open System
open System.Threading.Tasks
open Akka.Actor
open Akkling
open FSharp.Control.Tasks.V2.ContextInsensitive

[<AutoOpen>]
module TaskOfAskActor =

    /// Given actor `sendTo` with ask evt
    /// Will create transient actor `tellTo` and then ask `sendTo` with `evt`
    /// Then will wait `timeout` to receive response to `tellTo` and set one as Task result
    /// Task itself will be returned as function result
    let taskOfAskActor'<'a, 'b> (timeout: int option) (sys: IActorRefFactory) (sendTo: IActorRef<'b>) (evt): Task<'a> =
        let t = TaskCompletionSource<'a>()

        // actor will send message to target actor and then wait async result
        let actorProps =
            props
                (actorOf2 (fun a ->
                    function
                    | Start ->
                        async {
                            // send message to actor
                            let! res = sendTo <? evt
                            // async result acquired, set task result
                            t.TrySetResult(res) |> ignore
                            // stop actor
                            a.Self <! Complete
                        }
                        |> ignore
                        Ignore
                    | Complete -> Stop))

        let actor = spawnAnonymous sys actorProps
        actor <! Start

        let task = t.Task
        match timeout with
        | Some timeout -> waitActorWithTask actor task timeout
        | None -> task

    let taskOfAskActor x = x |> taskOfAskActor' None
