namespace PRR.API.Routes.Auth.RefreshToken

open Akkling
open System.Threading.Tasks
open PRR.System.Models
open PRR.Domain.Auth.RefreshToken

[<AutoOpen>]
module private OnSuccess =

    let onSuccess (sys: ICQRSSystem): OnSuccess =
        fun data ->
            sys.EventsRef <! (RefreshTokenSuccessEvent data)
            Task.FromResult()
