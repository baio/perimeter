namespace PRR.System.Models


[<AutoOpen>]
module Commands =
       
    type Commands =
        | SendConfirmEmailCommand of SignUpToken.Item
        | SendResetPasswordEmailCommand of ResetPassword.Item
        | CreateUserTenantCommand of SignUpConfirmSuccess
        | RefreshTokenCommand of RefreshToken.Commands
        | SignUpTokenCommand of SignUpToken.Commands
        | ResetPasswordCommand of ResetPassword.Commands
        | LogInCommand of LogIn.Commands
        | SSOCommand of SSO.Commands
