namespace PRR.Domain.Auth.LogOut

open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.System.Models
open System.Threading.Tasks

[<AutoOpen>]
module LogOut =

    let logout: LogOut =
        fun env data (userId, clientId) ->
            // TODO : Check allowed return url
            let result = { ReturnUri = data.ReturnUri }
            let evt = UserLogOutRequestedEvent(userId)
            Task.FromResult(result, evt)
