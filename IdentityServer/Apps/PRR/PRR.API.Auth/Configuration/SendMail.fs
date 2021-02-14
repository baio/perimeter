namespace PRR.API.Auth.Configuration

open FluentEmail.Core
open Microsoft.Extensions.DependencyInjection
open PRR.API.Auth.Infra.Mail
open PRR.API.Auth.Infra.SendMail
open FluentEmail.SendGrid
open FluentEmail.Handlebars

[<AutoOpen>]
module MailSender =

    let configureSendMail (sendGridApiKey: string) (config: MailSenderConfig) (services: IServiceCollection) =

        let sendMail =
            createSendMail config FluentMail.createSendMail

#if E2E
        let sandboxMode = true
#else
        let sandboxMode = false
#endif

        services
            .AddFluentEmail(config.FromEmail)
            .AddSendGridSender(sendGridApiKey, sandboxMode)
            .AddHandlebarsRenderer()
        |> ignore

        services.AddSingleton<ISendMailProvider>(SendMailProvider sendMail)
        |> ignore

        Email.DefaultSender <- SendGridSender(sendGridApiKey, sandboxMode)
        Email.DefaultRenderer <- HandlebarsRenderer()
