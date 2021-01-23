namespace PRR.API.Routes.Auth.Social

open Common.Domain.Giraffe
open PRR.API
open PRR.System.Models
open PRR.Domain.Auth.Social.SocialAuth
open Akkling
open System.Threading.Tasks
open PRR.API.Routes.DIHelpers
open PRR.Sys.Models.Social
open Akkling

[<AutoOpen>]
module private OnSuccess =

    let onSuccess ctx: OnSuccess =
        fun data ->
            let socialActor = (getSystemActors ctx).Social
            socialActor <! (SocialLoginAddCommand data)
            Task.FromResult()
