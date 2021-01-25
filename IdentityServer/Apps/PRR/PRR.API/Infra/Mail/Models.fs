namespace PRR.API.Infra.Mail

open System.Threading.Tasks
open PRR.System.Models

[<AutoOpen>]
module Models =
    [<CLIMutable>]
    type ProjectConfig =
        { Name: string
          BaseUrl: string
          ConfirmSignUpUrl: string
          ResetPasswordUrl: string }

    [<CLIMutable>]
    type MailSenderConfig =
        { Project: ProjectConfig
          FromEmail: string
          FromName: string }

    type SendMailData =
        { FromEmail: string
          FromName: string
          ToEmail: string
          ToName: string
          Subject: string
          Html: string }

    type MailSender = SendMailData -> Task<unit>

    type ISendMailProvider =
        abstract GetSendMail: unit -> SendMail

    type SendMailProvider(sendMail: SendMail) =
        interface ISendMailProvider with
            member __.GetSendMail() = sendMail
