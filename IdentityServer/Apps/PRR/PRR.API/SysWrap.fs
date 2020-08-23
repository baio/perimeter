namespace PRR.API

open Akkling
open Common.Domain.Models
open Common.Utils.TaskUtils
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Microsoft.AspNetCore.Http
open PRR.System
open PRR.System.Models
open System
open System.Threading.Tasks

[<AutoOpen>]
module SysWrap =

    type HandlerFunCmd = HttpContext -> Task<Commands>

    let sysWrapCmd (handler: HandlerFunCmd) (next: HttpFunc) (ctx: HttpContext) =
        let sys = ctx.GetService<ICQRSSystem>()
        task {
            let! cmd = handler ctx
            sys.CommandsRef <! cmd
            return! Successful.NO_CONTENT next ctx
        }

    type HandlerFun = HttpContext -> Task<Task<Events>>

    let sysWrap (handler: HandlerFun) (next: HttpFunc) (ctx: HttpContext) =
        let sys = ctx.GetService<ICQRSSystem>()
        task {
            let! hr = handler ctx
            let! evt = hr
            sys.EventsRef <! evt
            return! Successful.NO_CONTENT next ctx
        }

    type HandlerFun'<'a> = HttpContext -> Task<Task<'a * Events>>

    let sysWrapOK (handler: HandlerFun'<_>) (next: HttpFunc) (ctx: HttpContext) =
        let sys = ctx.GetService<ICQRSSystem>()
        task {
            let! hr = handler ctx
            let! (res, evt) = hr
            sys.EventsRef <! evt
            return! Successful.OK res next ctx
        }

    type GetResultUrlFun<'a> = HttpContext -> Result<'a, exn> -> string

    let sysWrapRedirect (m: GetResultUrlFun<'a>) (handler: HandlerFun'<'a>) (next: HttpFunc) (ctx: HttpContext) =
        let sys = ctx.GetService<ICQRSSystem>()
        task {
            try
                let! hr = handler ctx
                let! (res, evt) = hr
                sys.EventsRef <! evt
                let url =
                    res
                    |> Ok
                    |> m ctx
                return! redirectTo false url next ctx
            with err ->
                let url =
                    err
                    |> Error
                    |> m ctx
                return! redirectTo false url next ctx
        }

    let bindSysQuery f x (ctx: HttpContext) =
        let sys = ctx.GetService<ICQRSSystem>()
        let f' a = f (x, a)
        optionFromValueResult <!> taskOfQueryActor 500<milliseconds> sys.System sys.QueriesRef f' sys.EventsRef

    let sendEvent evt (ctx: HttpContext) =
        let sys = ctx.GetService<ICQRSSystem>()
        sys.EventsRef <! evt