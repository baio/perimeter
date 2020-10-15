namespace PRR.Sys.Social

open FSharp.Akkling.CQRS

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

    let handleSocialLoginQueryCommand state (token: Token) =
        let item = state |> Map.tryFind token
        match item with
        | Some _ -> queryResultMsgSome (SocialLoginRemoveCommand token) item
        | None -> queryResultMsgNone item

    let handleSocialLoginRemoved state (token: Token) =
        state |> Map.remove token |> stateMsgNone

    let reducer state =
        function
        | SocialLoginAddCommand socialLogin -> socialLogin |> SocialLoginAddedEvent |> PersistMsg
        | SocialLoginAddedEvent socialLogin -> handleSocialLoginAdded state socialLogin
        | SocialLoginQueryAndRemoveCommand token -> handleSocialLoginQueryCommand state token
        | SocialLoginRemoveCommand token -> token |> SocialLoginRemovedEvent |> PersistMsg
        | SocialLoginRemovedEvent token -> handleSocialLoginRemoved state token

    let createReducer () = msgReducer (Map.empty) reducer
