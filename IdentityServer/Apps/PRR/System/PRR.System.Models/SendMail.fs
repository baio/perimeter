namespace PRR.System.Models

[<AutoOpen>]
module SendMail =

    type ConfirmSignUpMailData = {
        FirstName: string
        LastName: string
        Email: string
        Token: string
        QueryString: string option
    }

    type SendMailTemplate =
        | ConfirmSignUpMail of ConfirmSignUpMailData
        | ResetPasswordMail of ResetPassword.Item

    type SendMailParams =
        { From: string
          To: string
          Subject: string
          Template: SendMailTemplate }




