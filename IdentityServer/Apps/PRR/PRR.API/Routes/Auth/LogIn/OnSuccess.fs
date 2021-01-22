namespace PRR.API.Routes.Auth.LogIn

open Akkling
open System.Threading.Tasks
open PRR.System.Models
open PRR.Domain.Auth.LogIn

[<AutoOpen>]
module private OnSuccess =

    let onSuccess (sys: ICQRSSystem): OnSuccess =
        fun data ->
            sys.EventsRef <! (UserLogInSuccessEvent data)
            Task.FromResult()
