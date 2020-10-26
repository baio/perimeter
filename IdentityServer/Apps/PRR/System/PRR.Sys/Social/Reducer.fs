namespace PRR.Sys.Social

open Akkling
open FSharp.Akkling.CQRS
open PRR.Sys.Models
open PRR.Sys.Models.Social

[<AutoOpen>]
module Reducer =

    type State = Map<Token, Item>

    let handleSocialLoginAdded state item =
        let updState = state |> Map.add item.Token item
        let delayCmd = item.Token |> SocialLoginRemoveCommand
        StateDelayMsg(updState, (float item.ExpiresIn, delayCmd))

    let handleSocialLoginQueryCommand state (token, (toActor: IActorRef<_>)) =
        let item = state |> Map.tryFind token
        toActor <! (token, item)
        NoneMsg

    let handleSocialLoginRemoved state (token: Token) =
        state |> Map.remove token |> stateMsgNone

    let reducer state =
        function
        | SocialLoginAddCommand socialLogin -> socialLogin |> SocialLoginAddedEvent |> PersistMsg
        | SocialLoginAddedEvent socialLogin -> handleSocialLoginAdded state socialLogin
        | SocialLoginQueryCommand q -> handleSocialLoginQueryCommand state q
        | SocialLoginRemoveCommand token -> token |> SocialLoginRemovedEvent |> PersistMsg
        | SocialLoginRemovedEvent token -> handleSocialLoginRemoved state token

    let createReducer () = msgReducer (Map.empty) reducer
