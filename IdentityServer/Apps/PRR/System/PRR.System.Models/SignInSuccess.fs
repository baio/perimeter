namespace PRR.System.Models

open Common.Domain.Models

[<AutoOpenAttribute>]
module SignInSuccess =
    type SignInSuccess = {
        ClientId: ClientId
        UserId: UserId
        RefreshToken: string
    }

