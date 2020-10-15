namespace FSharp.Akkling.CQRS

open System
open System.Threading.Tasks
open Akka.Actor
open Akkling
open FSharp.Control.Tasks.V2.ContextInsensitive

[<AutoOpen>]
module TaskToQueryActor =

    let private taskWithTimeout (timeout: int) (t: Task<'a>) =
        let tm =
            task {
                do! Task<'a>.Delay(int timeout)
                raise (TimeoutException "Timeout expired")
            }

        task {
            let! r = Task<'a>.WhenAny(t, tm)
            return! (r :?> Task<'a>)
        }

    type QueryActorEvent =
        | Start
        | Complete

    /// Given actor `sendTo` with ask evt
    /// Will create transient actor `tellTo` and then ask `sendTo` with `evt`
    /// Then will wait `timeout` to receive response to `tellTo` and set one as Task result
    /// Task itself will be returned as function result
    let taskOfQueryActor<'a, 'b> (timeout: int) (sys: IActorRefFactory) (sendTo: IActorRef<'b>) (evt): Task<'a> =
        let t = TaskCompletionSource<'a>()

        let actorProps =
            props
                (actorOf2 (fun a ->
                    function
                    | Start ->
                        async {
                            let! res = sendTo <? evt
                            t.TrySetResult(res) |> ignore
                            a.Self <! Complete
                        }
                        |> ignore
                        Ignore
                    | Complete -> Stop))

        let actor = spawnAnonymous sys actorProps
        actor <! Start
        task {
            try
                return! (t.Task |> taskWithTimeout timeout)
            with e ->
                (retype actor) <! PoisonPill.Instance
                return raise e
        }
