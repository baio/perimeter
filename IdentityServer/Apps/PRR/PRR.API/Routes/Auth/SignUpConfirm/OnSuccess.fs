namespace PRR.API.Routes.Auth.SignUpConfirm

open Akkling
open System.Threading.Tasks
open PRR.System.Models
open PRR.Domain.Auth.SignUpConfirm

[<AutoOpen>]
module private OnSuccess =

    let onSuccess fCreateTenant (sys: ICQRSSystem): OnSuccess =
        fun data ->
            sys.EventsRef
            <! (UserSignUpConfirmedEvent(data, fCreateTenant))
            Task.FromResult()
