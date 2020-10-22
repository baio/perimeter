namespace PRR.Sys.Models

open Akkling
open Common.Domain.Models
open Common.Domain.Models.SocialType

module Social =

    type Item =
        { Token: Token
          ClientId: string
          Type: SocialType
          ResponseType: string
          State: string
          RedirectUri: Uri
          Scope: Scope
          CodeChallenge: string
          CodeChallengeMethod: string }

    type Message =
        | SocialLoginAddCommand of Item
        | SocialLoginAddedEvent of Item
        | SocialLoginQueryCommand of (Token * IActorRef<Token * Item option>)
        | SocialLoginRemoveCommand of Token
        | SocialLoginRemovedEvent of Token
