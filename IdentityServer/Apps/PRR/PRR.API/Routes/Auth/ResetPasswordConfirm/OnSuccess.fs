namespace PRR.API.Routes.Auth.ResetPasswordConfirm

open Akkling
open System.Threading.Tasks
open PRR.System.Models
open PRR.Domain.Auth.ResetPassword

[<AutoOpen>]
module private OnSuccess =

    let onSuccess (sys: ICQRSSystem): OnSuccess =
        fun data ->
            sys.EventsRef <! (ResetPasswordUpdated data)
            Task.FromResult()
