namespace PRR.API.Routes.Auth.AuthorizeToken

open Akkling
open System.Threading.Tasks
open PRR.System.Models
open PRR.Domain.Auth.LogInToken

[<AutoOpen>]
module private OnSuccess =

    let onSuccess (sys: ICQRSSystem): OnSuccess =
        fun data ->
            sys.EventsRef <! (UserLogInTokenSuccessEvent data)
            Task.FromResult()
