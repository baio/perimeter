namespace Common.Domain.Giraffe

open Common.Utils

[<AutoOpen>]
module WrapHandler = 
    let wrapHandler fn next ctx =
        ctx
        |> fn
        |> TaskUtils.bind (fun hr -> hr ctx next)
    

