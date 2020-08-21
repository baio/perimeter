namespace PRR.System

open Akkling
open PRR.System.Models

[<AutoOpen>]
module private SharedActors =

    type SharedActors =
        { RefreshTokenActor: IActorRef<RefreshToken.Message>
          SignUpTokenActor: IActorRef<SignUpToken.Message>
          ResetPasswordActor: IActorRef<ResetPassword.Message>
          LogInActor: IActorRef<LogIn.Message>
          SSOActor: IActorRef<SSO.Message> }

    let createSharedActors sys (env: SystemEnv) (events: Lazy<IActorRef<Events>>) ss =
        let refreshTokenActor =
            spawn sys "refreshToken"
            <| { refreshToken env.AuthConfig.RefreshTokenExpiresIn with SupervisionStrategy = Some(ss) }

        let signUpTokenActor =
            let env: SignUpToken.Env =
                { TokenExpiresIn = env.AuthConfig.SignUpTokenExpiresIn
                  PasswordSalter = env.PasswordSalter }
            spawn sys "signUpToken" <| { signUpToken env events.Value with SupervisionStrategy = Some(ss) }

        let resetPasswordActor =
            spawn sys "resetPassword"
            <| { resetPassword
                     { HashProvider = env.HashProvider
                       TokenExpiresIn = env.AuthConfig.ResetPasswordTokenExpiresIn } events.Value with
                     SupervisionStrategy = Some(ss) }

        let logInActor =
            spawn sys "logIn" <| { logIn events.Value with SupervisionStrategy = Some(ss) }

        let ssoActor =
            spawn sys "sso" <| { sso events.Value with SupervisionStrategy = Some(ss) }
        
        { RefreshTokenActor = refreshTokenActor
          SignUpTokenActor = signUpTokenActor
          ResetPasswordActor = resetPasswordActor
          LogInActor = logInActor
          SSOActor = ssoActor }
