namespace PRR.System.Models

open Common.Domain.Models
open System
open Akkling


module RefreshToken =

    // Queries
    type Item =
        { Token: Token
          ClientId: ClientId
          UserId: UserId          
          SigningAudience: string
          ExpiresAt: DateTime
          Scopes: string seq }

    type GetRefreshTokenQueryResult = ValueResult<Item>

    type Queries = GetToken of Token * IActorRef<GetRefreshTokenQueryResult>

    // Commands
    type Commands =
        | Restart
        | AddToken of Item
        | RemoveToken of UserId
        | UpdateToken of RefreshTokenSuccess
        | MakeSnapshot

    // Events
    type Events =
        | TokenAdded of Item
        | TokenRemoved of Token
        | TokenUpdated of Item * Token

    // Messages
    type Message =
        | Command of Commands
        | Query of Queries
        | Event of Events
