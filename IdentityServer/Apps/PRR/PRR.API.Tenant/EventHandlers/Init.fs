namespace PRR.API.Tenant.EventHandlers

open MongoDB.Driver
open PRR.Domain.Tenant.Views.LogInView

[<AutoOpen>]
module Init =

    let initEventHandlers (db: IMongoDatabase) =
        initLoginView db
