namespace DataAvail.Common

module ReaderTask =
    
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open System.Threading.Tasks

    type Reader<'R,'T> = 'R -> 'T
    type ReaderTask<'e, 'r> = Reader<'e, Task<'r>>

    let map: ('a -> 'b) -> ReaderTask<'e, 'a> -> ReaderTask<'e, 'b> =
        fun fn x env ->
            task {
                let! x' = x env
                return fn x' }

    let ap: ReaderTask<'e, 'a -> 'b> -> ReaderTask<'e, 'a> -> ReaderTask<'e, 'b> =
        fun fn x env ->
            task {
                let! fn' = fn env
                let! x' = x env
                return fn' x' }

    let bind: ('a -> ReaderTask<'e, 'b>) -> ReaderTask<'e, 'a> -> ReaderTask<'e, 'b> =
        fun fn x env ->
            task {
                let! x' = x env
                let fn' = fn x'
                let! r = fn' env
                return r
            }

    let (<*>) = ap

    let (<!>) = map

    let (>>=) a b = bind b a

    let ofReader (r: Reader<'e, 'r>): ReaderTask<'e, 'r> =
        fun env -> Task.FromResult (r env)

    let ofTask (r: Task<'r>): ReaderTask<'e, 'r> =
        fun _ -> r

    let noneFails: 'ex -> ReaderTask<'e, 'r option> -> ReaderTask<'e, 'r> =
        fun ex x env ->
            task {
                match! x env with
                | Some r ->
                    return r
                | None ->
                    return! raise ex
            }
