namespace PRR.Domain.Auth.Social.SocialCallback
// TODO : Common.Domain.Models -> PRR.Common.Domain.Models
open Common.Domain.Models
open PRR.Domain.Auth.Social.SocialCallback.Identities

[<AutoOpen>]
module GetSocialIdentity =

    let getSocialIdentity =
        function
        | Github -> Twitter.Handler.getSocialIdentity
