namespace DataAvail.Common

[<AutoOpen>]
module TaskUtils =
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open System.Threading.Tasks

    let raiseTask x = Task.FromException<_>(x)

    let map f (x: Task<_>) =
        task {
            let! r = x
            return f r }

    let bind (f: 'a -> Task<'b>) (x: Task<'a>) =
        task {
            let! r = x
            return! f r }

    let inline (<!>) f (x: Task<_>) = map f x
    
    let inline (>>=) (x: Task<_>) f = bind f x
      
    let ignoreTask (x: Task<_>) = (fun _ -> ()) <!> x

    let returnM = Task.FromResult

    let option2Task noneEx =
        function
        | Some x -> Task.FromResult x
        | None -> raiseTask noneEx
