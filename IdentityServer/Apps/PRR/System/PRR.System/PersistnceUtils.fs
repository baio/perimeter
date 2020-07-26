namespace PRR.System

open Akkling.Persistence

[<AutoOpen>]
module PersistenceUtils =

    let isNotRecovering (ctx: Eventsourced<_>) = ctx.IsRecovering() |> not
