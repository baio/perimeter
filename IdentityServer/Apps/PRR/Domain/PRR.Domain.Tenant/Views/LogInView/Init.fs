﻿namespace PRR.Domain.Tenant.Views.LogInView

open Common.Domain.Models
open FSharp.MongoDB.Driver
open MongoDB.Driver
open PRR.Domain.Common.Events

module Init =

    let init (db: IMongoDatabase) =

        let col =
            db.GetCollection<LogIn>(LOGIN_VIEW_COLLECTION_NAME)

        let keys =
            [| createIndexModel <@ fun x -> x.DateTime @> BsonOrderDesc
               createIndexModel <@ fun x -> x.UserEmail @> BsonOrderAsc
               createIndexModel <@ fun x -> x.IsManagementClient @> BsonOrderAsc |]

        createIndexesRange col keys |> ignore
