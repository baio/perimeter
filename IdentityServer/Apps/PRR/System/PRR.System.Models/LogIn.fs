namespace PRR.System.Models

open System
open Akkling
open Common.Domain.Models

module LogIn =

    type Item =
        { Code: Token
          ClientId: ClientId
          CodeChallenge: Token
          ExpiresAt: DateTime }

    // Queries
    type GetLogInCodeQueryResult = ValueResult<Item>

    type Queries = GetCode of Token * IActorRef<GetLogInCodeQueryResult>
    
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

