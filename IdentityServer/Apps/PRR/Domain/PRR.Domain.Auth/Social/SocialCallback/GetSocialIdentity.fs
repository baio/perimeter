namespace PRR.Domain.Auth.Social.SocialCallback
// TODO : PRR.Domain.Models -> PRR.PRR.Domain.Models
open System
open PRR.Domain.Models
open PRR.Domain.Auth.Social.SocialCallback.Identities

[<AutoOpen>]
module GetSocialIdentity =

    let getSocialIdentity redirectUri =
        function
        | Github -> Github.Handler.getSocialIdentity
        | Google -> Google.Handler.getSocialIdentity redirectUri
        | Twitter -> raise (NotImplementedException "not implemented")
