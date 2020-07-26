open Xunit

[<assembly:CollectionBehavior(DisableTestParallelization = true)>]
do ()

module Program = let [<EntryPoint>] main _ = 0
