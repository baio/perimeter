namespace PRR.System

open Akkling
open PRR.System.Models

[<AutoOpen>]
module private SharedActors =

    type SharedActors =
        { RefreshTokenActor: IActorRef<RefreshToken.Message>
          SignUpTokenActor: IActorRef<SignUpToken.Message>
          ResetPasswordActor: IActorRef<ResetPassword.Message> }

    let createSharedActors sys (env: SystemEnv) (events: Lazy<IActorRef<Events>>) ss =
        let refreshTokenActor =
            spawn sys "refreshToken"
            <| { refreshToken env.AuthConfig.RefreshTokenExpiresIn with SupervisionStrategy = Some(ss) }

        let signUpTokenActor =
            spawn sys "signUpToken"
            <| { signUpToken env.AuthConfig.SignUpTokenExpiresIn events.Value with SupervisionStrategy = Some(ss) }

        let resetPasswordActor =
            spawn sys "resetPassword"
            <| { resetPassword
                     { HashProvider = env.HashProvider
                       TokenExpiresIn = env.AuthConfig.ResetPasswordTokenExpiresIn } events.Value with
                     SupervisionStrategy = Some(ss) }

        { RefreshTokenActor = refreshTokenActor
          SignUpTokenActor = signUpTokenActor
          ResetPasswordActor = resetPasswordActor }
