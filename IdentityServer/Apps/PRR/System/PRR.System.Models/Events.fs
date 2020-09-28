namespace PRR.System.Models

open Akkling.ActorRefs
open Common.Domain.Models
open System

[<AutoOpen>]
module Events =

    type LoginSuccessData =
        { DomainId: int
          AppIdentifier: string
          UserEmail: string
          Date: DateTime }

    type Events =
        | UserSignedUpEvent of SignUpSuccess
        | UserSignUpConfirmedEvent of SignUpConfirmSuccess * bool
        | UserTenantCreatedEvent of CreatedTenantInfo
        | RefreshTokenSuccessEvent of RefreshTokenSuccess
        | SignUpTokenEvent of SignUpToken.Events
        | ResetPasswordEvent of ResetPassword.Events
        | UserLogInSuccessEvent of LogIn.Item * (SSO.Item option)
        | UserLogInTokenSuccessEvent of Token * RefreshToken.Item * LoginSuccessData
        | LogInEvent of LogIn.Events
        | SSOEvent of SSO.Events
        | ResetPasswordRequested of Email
        | ResetPasswordUpdated of Email
        | CommandFailureEvent of exn
        | QueryFailureEvent of exn
        | UserLogOutRequestedEvent of UserId
