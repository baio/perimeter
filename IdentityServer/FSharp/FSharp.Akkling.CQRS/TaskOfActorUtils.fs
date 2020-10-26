namespace FSharp.Akkling.CQRS

open System
open System.Threading.Tasks
open Akka.Actor
open Akkling.ActorRefs
open FSharp.Control.Tasks.V2.ContextInsensitive

[<AutoOpen>]
module internal TaskOfActorUtils =

    let taskWithTimeout (timeout: int) (t: Task<'a>) =
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


    let waitActorWithTask (actor: IActorRef<'b>) (t: Task<'a>) (timeout: int): Task<'a> =
        task {
            try
                // can't receive result in timeout, return timeout error
                return! (t |> taskWithTimeout timeout)
            with e ->
                // stop actor
                (retype actor) <! PoisonPill.Instance

                return raise e
        }
