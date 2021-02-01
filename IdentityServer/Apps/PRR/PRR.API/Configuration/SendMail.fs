namespace PRR.API.Configuration

open Microsoft.Extensions.DependencyInjection
open PRR.API.Infra.Mail
open PRR.API.Infra.Mail.Models
open PRR.API.Infra.SendMail

[<AutoOpen>]
module MailSender =

    let configureSendMail (sendGridApiKey: string) (config: MailSenderConfig) (services: IServiceCollection) =

        let mailSender =
            SendGridMail.createSendMail sendGridApiKey

        let sendMail = createSendMail config mailSender

        services.AddSingleton<ISendMailProvider>(SendMailProvider sendMail)
        |> ignore
