namespace PRR.API.Routes.Auth.LogOut

open Common.Domain.Giraffe
open PRR.API
open PRR.System.Models
open PRR.Domain.Auth.LogOut
open Akkling
open System.Threading.Tasks

[<AutoOpen>]
module private OnSuccess =

    let onSuccess (sys: ICQRSSystem): OnSuccess =
        fun data ->
            sys.EventsRef <! (UserLogOutRequestedEvent(data))
            Task.FromResult()
