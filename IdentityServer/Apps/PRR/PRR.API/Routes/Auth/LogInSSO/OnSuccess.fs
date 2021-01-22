namespace PRR.API.Routes.Auth.LogInSSO

open System.Threading.Tasks
open Akkling
open PRR.System.Models
open PRR.Domain.Auth.LogInSSO

[<AutoOpen>]
module private OnSuccess =

    let onSuccess (sys: ICQRSSystem): OnSuccess =
        fun data ->
            sys.EventsRef
            <! (UserLogInSuccessEvent(data, None))
            Task.FromResult()
