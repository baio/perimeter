namespace PRR.Domain.Auth.Social.SocialCallback
// TODO : Common.Domain.Models -> PRR.Common.Domain.Models
open System
open Common.Domain.Models
open PRR.Domain.Auth.Social.SocialCallback.Identities

[<AutoOpen>]
module GetSocialIdentity =

    let getSocialIdentity redirectUri =
        function
        | Github -> Github.Handler.getSocialIdentity
        | Google -> Google.Handler.getSocialIdentity redirectUri
        | Twitter -> raise (NotImplementedException "not implemented")
