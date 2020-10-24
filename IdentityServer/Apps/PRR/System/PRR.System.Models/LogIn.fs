namespace PRR.System.Models

open System
open Akkling
open Common.Domain.Models

module LogIn =

    type LoginSuccessData =
        { IsManagementClient: bool
          DomainId: int
          AppIdentifier: string
          UserEmail: string
          Date: DateTime }

    type Item =
        { Code: Token
          UserId: UserId
          Social: Social option
          RequestedScopes: string seq
          ValidatedScopes: AudienceScopes seq
          RedirectUri: string
          ClientId: ClientId
          Issuer: string
          CodeChallenge: Token
          ExpiresAt: DateTime }

    // Queries
    type GetLogInCodeQueryResult = ValueResult<Item>

    type Queries = GetCode of Token * IActorRef<GetLogInCodeQueryResult>

    // Commands
    type Commands =
        | Restart
        | AddCode of Item
        | RemoveCode of Token * LoginSuccessData
        | MakeSnapshot

    // Events
    type Events =
        | CodeAdded of Item
        | CodeRemoved of Token * LoginSuccessData

    // Messages
    type Message =
        | Command of Commands
        | Query of Queries
        | Event of Events
