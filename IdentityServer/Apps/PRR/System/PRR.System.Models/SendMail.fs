namespace PRR.System.Models

[<AutoOpen>]
module SendMail =

    type SendMailTemplate =
        | ConfirmSignUpMail of SignUpToken.Item
        | ResetPasswordMail of ResetPassword.Item

    type SendMailParams =
        { From: string
          To: string
          Subject: string
          Template: SendMailTemplate }




