namespace PRR.Sys.Social

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
        | SocialLoginQueryAndRemoveCommand of Token
        | SocialLoginRemoveCommand of Token
        | SocialLoginRemovedEvent of Token
