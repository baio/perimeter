namespace PRR.Sys.Models

open Akkling

module Social =

    type Github = | Github

    type SocialLogin =
        { Token: Token
          ClientId: string
          Type: Github }

    type Item = { ClientId: string; Type: Github }

    type Message =
        | SocialLoginAddCommand of SocialLogin
        | SocialLoginAddedEvent of SocialLogin
        | SocialLoginQueryAndRemoveCommand of (Token * IActorRef<Token * Item option>)
        | SocialLoginRemoveCommand of Token
        | SocialLoginRemovedEvent of Token
