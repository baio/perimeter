namespace PRR.API.Routes.Auth.SocialCallback

open PRR.API.Routes.Auth
open PRR.Domain.Auth.Social.SocialCallback
open PRR.System.Models

[<AutoOpen>]
module private OnSuccess =

    let onSuccess (env: LogIn.OnSuccess.Env): OnSuccess = LogIn.OnSuccess.onSuccess env
