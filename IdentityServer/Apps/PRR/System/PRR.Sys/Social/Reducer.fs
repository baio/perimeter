namespace PRR.Sys.Social

open Akkling
open FSharp.Akkling.CQRS
open PRR.Sys.Models
open PRR.Sys.Models.Social

[<AutoOpen>]
module Reducer =

    type State = Map<Token, Item>

    let handleSocialLoginAdded state socialLogin =
        state
        |> Map.add
            socialLogin.Token
               { ClientId = socialLogin.ClientId
                 Type = socialLogin.Type }
        |> stateMsgNone

    let handleSocialLoginQueryCommand state (token, (toActor: IActorRef<_>)) =
        let item = state |> Map.tryFind token
        toActor <! (token, item)
        match item with
        | Some _ -> JustMsg(SocialLoginRemoveCommand token)
        | None -> NoneMsg

    let handleSocialLoginRemoved state (token: Token) =
        state |> Map.remove token |> stateMsgNone

    let reducer state =
        function
        | SocialLoginAddCommand socialLogin -> socialLogin |> SocialLoginAddedEvent |> PersistMsg
        | SocialLoginAddedEvent socialLogin -> handleSocialLoginAdded state socialLogin
        | SocialLoginQueryAndRemoveCommand q -> handleSocialLoginQueryCommand state q
        | SocialLoginRemoveCommand token -> token |> SocialLoginRemovedEvent |> PersistMsg
        | SocialLoginRemovedEvent token -> handleSocialLoginRemoved state token

    let createReducer () = msgReducer (Map.empty) reducer
