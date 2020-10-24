namespace Common.Domain.Models

[<AutoOpen>]
module Social =

    type SocialType = | Github

    type Social = { Id: string; Type: SocialType }
