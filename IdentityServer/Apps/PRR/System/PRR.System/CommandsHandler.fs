namespace PRR.System

open Akkling
open Common.Domain.Models.Models
open Common.Utils
open PRR.System.Models

[<AutoOpen>]
module private CommandsHandler =
    let commandsHandler env (eventsActor: Lazy<IActorRef<Events>>) (sharedActors: SharedActors) (ctx: Actor<_>) =

        let sendConfirmSignUpEmailActor =
            spawnAnonymous ctx <| props (actorOf (sendConfirmSignUpEmail env.SendMail >> ignored))

        let sendResetPasswordEmailActor =
            spawnAnonymous ctx <| props (actorOf (sendResetPasswordEmail env.SendMail >> ignored))

        let createUserTenantActor =
            spawnAnonymous ctx
            <| props
                (commandActorOfTask 500<milliseconds> eventsActor.Value
                     (createUserTenant
                         { GetDataContextProvider = env.GetDataContextProvider
                           HashProvider = env.HashProvider
                           AuthConfig = env.AuthConfig }
                      >> map UserTenantCreatedEvent))

        let rec loop() =
            actor {
                let! cmd = ctx.Receive()
                match cmd with
                | SendConfirmEmailCommand data ->
                    sendConfirmSignUpEmailActor <! data
                | SendResetPasswordEmailCommand data ->
                    sendResetPasswordEmailActor <! data
                | CreateUserTenantCommand data ->
                    createUserTenantActor <! data                    
                | RefreshTokenCommand cmd ->
                    sharedActors.RefreshTokenActor <! (RefreshToken.Command cmd)
                | SignUpTokenCommand cmd ->
                    sharedActors.SignUpTokenActor <! (SignUpToken.Command cmd)
                | ResetPasswordCommand cmd ->
                    sharedActors.ResetPasswordActor <! (ResetPassword.Command cmd)                    
                return loop()
            }
        loop()
