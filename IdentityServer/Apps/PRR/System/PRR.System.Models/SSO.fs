﻿namespace PRR.System.Models

open Common.Domain.Models
open System

module SSO =

    type Item =
        { Code: Token
          TenantId: TenantId
          UserId: UserId
          Email: string
          Social: Social option
          ExpiresAt: DateTime }

    // Queries
    type GetSSOCodeQueryResult = ValueResult<Item>

    type Queries = GetCode of Token * GetSSOCodeQueryResult

    // Commands
    type Commands =
        | Restart
        | AddCode of Item
        | RemoveCode of UserId
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
