namespace PRR.API.Routes.Auth.ResetPassword

open Akkling
open System.Threading.Tasks
open PRR.System.Models
open PRR.Domain.Auth.ResetPassword

[<AutoOpen>]
module private OnSuccess =

    let onSuccess (sys: ICQRSSystem): OnSuccess =
        fun email ->
            sys.EventsRef <! (ResetPasswordRequested email)
            Task.FromResult()
