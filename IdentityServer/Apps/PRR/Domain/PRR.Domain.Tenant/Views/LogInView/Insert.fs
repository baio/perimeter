namespace PRR.Domain.Tenant.Views.LogInView

open System
open DataAvail.KeyValueStorage.Mongo.Tests
open MongoDB.Bson
open PRR.Domain.Models
open FSharp.MongoDB.Driver
open MongoDB.Driver
open PRR.Domain.Common.Events

[<AutoOpen>]
module Insert =

    let private mapGrantType =
        function
        | LogInGrantType.AuthorizationCode data ->
            {| UserId = data.UserId
               UserEmail = data.UserEmail
               GrantType = "AuthorizationCode" |}
        | LogInGrantType.AuthorizationCodePKCE data ->
            {| UserId = data.UserId
               UserEmail = data.UserEmail
               GrantType = "AuthorizationCodePKCE" |}
        | LogInGrantType.Password data ->
            {| UserId = data.UserId
               UserEmail = data.UserEmail
               GrantType = "Password" |}
        | LogInGrantType.Social (data, _) ->
            {| UserId = data.UserId
               UserEmail = data.UserEmail
               GrantType = "Social" |}
        | LogInGrantType.ClientCredentials ->
            {| UserId = -1
               UserEmail = null
               GrantType = "ClientCredentials" |}

    let insertLoginEvent (db: IMongoDatabase) (evt: LogInEvent) =

        let grantData = mapGrantType evt.GrantType

        let doc: LogInDoc =
            { // Id = null
              // Denormalized version of Login event
              Social =
                  match evt.GrantType with
                  | LogInGrantType.Social (_, social) -> Some social
                  | _ -> None
              DateTime = evt.DateTime
              ClientId = evt.ClientId
              DomainId = evt.DomainId
              IsManagementClient = evt.IsManagementClient
              AppIdentifier = evt.AppIdentifier
              UserId = grantData.UserId
              UserEmail = grantData.UserEmail
              GrantType = grantData.GrantType }

        let col =
            db.GetCollection<LogInDoc>(LOGIN_VIEW_COLLECTION_NAME)

        insertOneAsync col doc
