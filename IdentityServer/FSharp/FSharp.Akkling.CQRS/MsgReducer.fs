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

    // helpers
    let stateMsgNone state = StateMsg(state, None)

    let msgReducer initState reducer =
        propsPersist (fun ctx ->
            let rec loop state =
                actor {
                    let! msg' = ctx.Receive()
                    let msg = reducer state msg'

                    match msg with
                    | NoneMsg -> return! loop state
                    | PersistMsg msg -> return! Persist msg
                    | JustMsg msg ->
                        ctx.Self <! msg
                        return! loop state
                    | StateMsg (updState, msg) ->
                        match msg with
                        | Some msg -> ctx.Self <! msg
                        | None -> ()
                        return! loop updState
                }

            loop initState)
