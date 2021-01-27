﻿namespace PRR.Domain.Auth.Common

open System.Threading.Tasks

[<AutoOpen>]
module SendMail =

    type ConfirmSignUpMailData =
        { FirstName: string
          LastName: string
          Email: string
          Token: string
          QueryString: string option }

    type ResetPasswordMailData = { Email: string; Token: string }


    type SendMailTemplate =
        | ConfirmSignUpMail of ConfirmSignUpMailData
        | ResetPasswordMail of ResetPasswordMailData

    type SendMailParams =
        { From: string
          To: string
          Subject: string
          Template: SendMailTemplate }

    type SendMail = SendMailParams -> Task<unit>