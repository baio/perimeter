﻿namespace PRR.Domain.Auth.Common

open System.Threading.Tasks
open Microsoft.Extensions.Logging

[<AutoOpen>]
module SendMail =

    type ConfirmSignUpMailData =
        { FirstName: string
          LastName: string
          Email: string
          Token: string
          RedirectUri: string }

    type ResetPasswordMailData =
        { Email: string
          Token: string
          RedirectUri: string }

    type SendMailTemplate =
        | ConfirmSignUpMail of ConfirmSignUpMailData
        | ResetPasswordMail of ResetPasswordMailData

    type SendMailParams =
        { From: string
          To: string
          Subject: string
          Template: SendMailTemplate }

    type SendMail = ILogger -> SendMailParams -> Task<unit>
