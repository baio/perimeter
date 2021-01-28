namespace PRR.Domain.Tenant.Views.LogInView

open Common.Domain.Models
open FSharp.MongoDB.Driver
open MongoDB.Driver
open PRR.Domain.Common.Events

[<AutoOpen>]
module Insert =

    let insertLoginEvent (db: IMongoDatabase) (evt: LogIn) =
        let col =
            db.GetCollection<LogIn>(LOGIN_VIEW_COLLECTION_NAME)

        insertOneAsync col evt
