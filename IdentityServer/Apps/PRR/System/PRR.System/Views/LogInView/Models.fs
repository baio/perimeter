namespace PRR.System.Views.LogInView
open FSharp.MongoDB.Driver
open MongoDB.Bson

[<AutoOpen>]
module Models =
       
    let LOGIN_VIEW_COLLECTION_NAME = "login_view"

    type Doc =
        { _id: string
          email: string
          dateTime: System.DateTime
          domainId: int
          appIdentifier: string
          isManagementClient: bool
          seqNr: int64 }
