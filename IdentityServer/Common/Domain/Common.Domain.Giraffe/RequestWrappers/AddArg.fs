namespace Common.Domain.Giraffe

open Common.Domain.Models
open Common.Utils

[<AutoOpen>]
module AddArg =

    let option2Task noneEx =
        function
        | Some x -> returnM x
        | None -> raiseTask noneEx
