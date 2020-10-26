namespace FSharp.Akkling.CQRS

open System.Threading.Tasks
open Akkling
open Akkling.Persistence

[<AutoOpen>]
module MsgReducer =

    type Msg<'s, 'm> =
        | NoneMsg
        | TaskMsg of Task<'m>
        | PersistMsg of 'm
        | JustMsg of 'm
        | StateMsg of 's * ('m option)
        | StateDelayMsg of 's * (float * 'm)

    // helpers
    let stateMsgNone state = StateMsg(state, None)

    let msgReducer initState reducer =
        propsPersist (fun ctx ->
            let rec loop state =

                actor {
                    let! msg' = ctx.Receive()
                    let msg = reducer state msg'

                    match msg with
                    | NoneMsg -> return ignored ()
                    | PersistMsg msg -> return Persist msg
                    | JustMsg msg ->
                        ctx.Self <! msg
                        return ignored ()
                    | StateMsg (updState, msg) ->
                        match msg with
                        | Some msg -> ctx.Self <! msg
                        | None -> ()

                        return loop updState
                    | StateDelayMsg (updState, (delay, msg)) ->
                        let timespan =
                            System.TimeSpan.FromMilliseconds(float delay)

                        ctx.System.Scheduler.ScheduleTellOnce(timespan, ctx.Self, msg)

                        return loop updState
                }

            loop initState)
