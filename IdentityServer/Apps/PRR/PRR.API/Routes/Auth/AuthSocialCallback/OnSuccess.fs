namespace PRR.API.Routes.Auth.SocialCallback

open PRR.Domain.Auth.Social.SocialCallback
open Akkling
open System.Threading.Tasks
open PRR.API.Routes.DIHelpers
open PRR.System.Models

[<AutoOpen>]
module private OnSuccess =

    let onSuccess ctx: OnSuccess =
        fun data ->
            let sys = getCQRSSystem ctx
            sys.EventsRef <! UserLogInSuccessEvent(data)
            Task.FromResult()
