namespace PRR.System.Models

open Common.Domain.Models

[<AutoOpen>]
module RefreshTokenSuccess =

    type RefreshTokenSuccess = {
        ClientId: ClientId
        UserId: UserId
        RefreshToken: string
        OldRefreshToken: string
        Scopes: string seq
    }



