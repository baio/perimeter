namespace PRR.Domain.Tenant.Views.LogInView

open Common.Domain.Models
open FSharp.MongoDB.Driver
open MongoDB.Driver
open PRR.Domain.Common.Events

module Insert =

    let insert (db: IMongoDatabase) (evt: LogIn) =
        let col =
            db.GetCollection<LogIn>(LOGIN_VIEW_COLLECTION_NAME)

        insertOneAsync col evt
