namespace PRR.Sys.Models

open Common.Domain.Models

module Social =

    type Item =
        { Token: Token
          ExpiresIn: int<milliseconds>
          SocialClientId: string
          Type: SocialType
          ResponseType: string
          State: string
          RedirectUri: Uri
          Scope: Scope
          CodeChallenge: string
          CodeChallengeMethod: string
          DomainClientId: ClientId }

    type Message =
        | SocialLoginAddCommand of Item
        | SocialLoginAddedEvent of Item
        | SocialLoginQueryCommand of Token
        | SocialLoginRemoveCommand of Token
        | SocialLoginRemovedEvent of Token
