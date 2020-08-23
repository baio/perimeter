namespace PRR.System

open Akkling
open PRR.System.Models


[<AutoOpen>]
module private EventsHandler =

    let ofOne item = seq { yield item }

    let mapEventToCommand evt =
        match evt with
        | SignUpTokenEvent(SignUpToken.TokenAdded(data)) ->
            data
            |> SendConfirmEmailCommand
            |> ofOne
        | ResetPasswordEvent(ResetPassword.TokenAdded(data)) ->
            data
            |> SendResetPasswordEmailCommand
            |> ofOne
        | UserSignUpConfirmedEvent data ->
            seq {
                data |> CreateUserTenantCommand
                data.Email
                |> SignUpToken.RemoveTokensWithEmail
                |> SignUpTokenCommand
            }
        | ResetPasswordRequested email ->
            email
            |> ResetPassword.AddToken
            |> ResetPasswordCommand
            |> ofOne
        | ResetPasswordUpdated email ->
            email
            |> ResetPassword.RemoveTokensWithEmail
            |> ResetPasswordCommand
            |> ofOne
        | UserTenantCreatedEvent _ ->
            Seq.empty
        | RefreshTokenSuccessEvent data ->
            data
            |> RefreshToken.UpdateToken
            |> RefreshTokenCommand
            |> ofOne
        | UserSignedUpEvent data ->
            data
            |> SignUpToken.AddToken
            |> SignUpTokenCommand
            |> ofOne
        | UserLogInSuccessEvent(loginItem, ssoItem) ->
            seq {
                loginItem
                |> LogIn.AddCode
                |> LogInCommand

                match ssoItem with
                | Some ssoItem ->
                    ssoItem
                    |> SSO.AddCode
                    |> SSOCommand
            }
        | UserLogInTokenSuccessEvent(token, item) ->
            seq {
                token
                |> LogIn.RemoveCode
                |> LogInCommand

                item
                |> RefreshToken.AddToken
                |> RefreshTokenCommand
            }
        | UserLogOutRequestedEvent(userId) ->
            seq {
                userId
                |> SSO.RemoveCode
                |> SSOCommand

                userId
                |> RefreshToken.RemoveToken
                |> RefreshTokenCommand
            }
        | CommandFailureEvent data ->
            printf "Command fails %O" data
            Seq.empty
        | QueryFailureEvent data ->
            printf "Query fails %O" data
            Seq.empty
        | _ ->
            printf "Event not handled %O" evt
            Seq.empty

    let eventsHandler env (commandsRef: Lazy<IActorRef<Commands>>) (sys: Actor<_>) =
        let rec loop() =
            actor {
                let! evt = sys.Receive()
                mapEventToCommand evt |> Seq.iter (fun cmd -> commandsRef.Value <! cmd)
                env.EventHandledCallback evt
                return loop()
            }
        loop()
