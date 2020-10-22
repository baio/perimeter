namespace FSharp.Akkling.CQRS

open System
open System.Threading.Tasks
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

        
