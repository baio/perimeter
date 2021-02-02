namespace PRR.API.Auth.Configuration

open Microsoft.Extensions.DependencyInjection
open PRR.API.Auth.Infra.Mail
open PRR.API.Auth.Infra.SendMail

[<AutoOpen>]
module MailSender =

    let configureSendMail (sendGridApiKey: string) (config: MailSenderConfig) (services: IServiceCollection) =

        let mailSender =
            SendGridMail.createSendMail sendGridApiKey

        let sendMail = createSendMail config mailSender

        services.AddSingleton<ISendMailProvider>(SendMailProvider sendMail)
        |> ignore
