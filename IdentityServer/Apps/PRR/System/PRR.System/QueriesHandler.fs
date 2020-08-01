namespace PRR.System

open Akkling
open PRR.System.Models

[<AutoOpen>]
module private QueriesHandler =

    let queriesHandler (sharedActors: SharedActors) (ctx: Actor<_>) =

        let rec loop() =
            actor {
                let! q = ctx.Receive()
                match q with
                | RefreshToken q ->
                    sharedActors.RefreshTokenActor <! RefreshToken.Query(q)
                | SignUpToken q ->
                    sharedActors.SignUpTokenActor <! SignUpToken.Query(q)
                | ResetPassword q ->
                    sharedActors.ResetPasswordActor <! ResetPassword.Query(q)
                | LogIn q ->
                    sharedActors.LogInActor <! LogIn.Query(q)
                return loop()
            }
        loop()
