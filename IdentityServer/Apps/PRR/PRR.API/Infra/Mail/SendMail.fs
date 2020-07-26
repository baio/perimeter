namespace PRR.API.Infra

open System
open PRR.System.Models

[<AutoOpen>]
module SendMail =
    let sendMail: SendMail =
        fun prms ->
            printf "sendMail: %O" prms
            System.Threading.Tasks.Task.FromResult(())
