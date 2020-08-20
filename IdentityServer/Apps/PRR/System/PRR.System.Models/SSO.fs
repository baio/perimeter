namespace PRR.System.Models

open Akkling
open Common.Domain.Models
open System

module SSO =

    type Item =
        { Code: Token
          Email: string
          RedirectUri: string
          ExpiresAt: DateTime }

    // Queries
    type GetSSOCodeQueryResult = ValueResult<Item>

    type Queries = GetCode of Token * IActorRef<GetSSOCodeQueryResult>

    // Commands
    type Commands =
        | Restart
        | AddCode of Item
        | RemoveCode of Token
        | MakeSnapshot

    // Events
    type Events =
        | CodeAdded of Item
        | CodeRemoved of Token

    // Messages
    type Message =
        | Command of Commands
        | Query of Queries
        | Event of Events
