namespace PRR.System.Models


open Akkling
open Common.Domain.Models
open System

module ResetPassword =
    
    // Models    
    type Item = {
        Email: Email
        Token: Token
        ExpiredAt: DateTime
    }

    // Queries

    type GetTokenQueryResult = ValueResult<Item>

    type Queries = GetToken of Token * IActorRef<GetTokenQueryResult>
    
    // Commands
    type Commands =
        | Restart
        | AddToken of Email
        | RemoveTokensWithEmail of Email
        | MakeSnapshot

    // Events
    type Events =        
        | TokenAdded of Item
        | TokenRemoved of Token
        
    // Messages    
    type Message =
        | Command of Commands
        | Query of Queries
        | Event of Events


