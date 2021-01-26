﻿namespace PRR.System.Models

open Common.Domain.Models
open System


module SignUpToken =
    
    // Models
    
    type Item = {
        FirstName: string
        LastName: string
        Email: string
        Password: string
        Token: string
        ExpiredAt: DateTime
        QueryString: string option
    }

    // Queries

    type GetSignUpTokenQueryResult = ValueResult<Item>

    type Queries = GetToken of Token * GetSignUpTokenQueryResult
    
    // Commands
    type Commands =
        | Restart
        | AddToken of SignUpSuccess
        | RemoveTokensWithEmail of string
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
        
