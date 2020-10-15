namespace PRR.Sys.Social

open Akkling

[<AutoOpen>]
module Messages =

    type Token = string

    type SocialType = | Twitter

    type SocialLogin =
        { Token: Token
          ClientId: string
          Type: SocialType }

    type Item = { ClientId: string; Type: SocialType }

    type Message =
        | SocialLoginAddCommand of SocialLogin
        | SocialLoginAddedEvent of SocialLogin
        | SocialLoginQueryAndRemoveCommand of (Token * IActorRef<Token * Item option>)
        | SocialLoginRemoveCommand of Token
        | SocialLoginRemovedEvent of Token
