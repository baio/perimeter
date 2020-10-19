namespace PRR.API.Infra

open PRR.Sys.SetUp

[<AutoOpen>]
module SystemActorsProvider =

    type SystemActorsProvider(systemActors: SystemActors) =
        interface ISystemActorsProvider with
            member __.SystemActors = systemActors
