namespace PRR.System.Models

open Akkling.ActorRefs
open Common.Domain.Models
open System

[<AutoOpen>]
module Events =
    
    type Events =
        | UserSignedUpEvent of SignUpSuccess
        | UserSignUpConfirmedEvent of SignUpConfirmSuccess
        | UserTenantCreatedEvent of CreatedTenantInfo
        | UserSignInSuccessEvent of SignInSuccess
        | RefreshTokenSuccessEvent of RefreshTokenSuccess
        | SignUpTokenEvent of SignUpToken.Events
        | ResetPasswordEvent of ResetPassword.Events
        | UserLogInSuccessEvent of LogIn.Item
        | LogInEvent of LogIn.Events 
        | ResetPasswordRequested of Email
        | ResetPasswordUpdated of Email
        | CommandFailureEvent of exn
        | QueryFailureEvent of exn 
         
