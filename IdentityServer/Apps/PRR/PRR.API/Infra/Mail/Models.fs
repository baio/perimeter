namespace PRR.API.Infra.Mail

open System.Threading.Tasks

[<AutoOpen>]
module Models =
    [<CLIMutable>]
    type ProjectEnv =
        { Name: string
          BaseUrl: string
          ConfirmSignUpUrl: string
          ResetPasswordUrl: string }

    [<CLIMutable>]
    type MailEnv =
        { Project: ProjectEnv
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
