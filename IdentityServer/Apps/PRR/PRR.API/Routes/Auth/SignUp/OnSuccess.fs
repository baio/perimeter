namespace PRR.API.Routes.Auth.SignUp

open Akkling
open System.Threading.Tasks
open PRR.System.Models
open PRR.Domain.Auth.SignUp

[<AutoOpen>]
module private OnSuccess =

    let onSuccess (sys: ICQRSSystem): OnSuccess =
        fun data ->
            sys.EventsRef <! (UserSignedUpEvent data)
            Task.FromResult()
