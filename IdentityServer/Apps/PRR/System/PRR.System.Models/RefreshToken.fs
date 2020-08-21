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
          ExpiresAt: DateTime }

    type GetRefreshTokenQueryResult = ValueResult<Item>

    type Queries = GetToken of Token * IActorRef<GetRefreshTokenQueryResult>
    
    // Commands
    type Commands =
        | Restart
        | AddToken of Item
        | UpdateToken of RefreshTokenSuccess
        | MakeSnapshot

    // Events
    type Events =
        | TokenAdded of Item
        | TokenUpdated of Item * Token
        
    // Messages    
    type Message =
        | Command of Commands
        | Query of Queries
        | Event of Events
        
