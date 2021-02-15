namespace PRR.API.Auth.Configuration

open FluentEmail.Core
open Microsoft.Extensions.DependencyInjection
open PRR.API.Auth.Infra.Mail
open PRR.API.Auth.Infra.SendMail
// open FluentEmail.SendGrid
open FluentEmail.Mailgun
open FluentEmail.Handlebars

[<AutoOpen>]
module MailSender =

    type SendMailConfig =
        { DomainName: string
          ApiKey: string
          Region: string }

    let configureSendMail (sendMailConfig: SendMailConfig) (config: MailSenderConfig) (services: IServiceCollection) =

        let sendMail =
            createSendMail config FluentMail.createSendMail

#if E2E
        let sandboxMode = true
#else
        let sandboxMode = false
#endif

        let domainName = sendMailConfig.DomainName
        let apiKey = sendMailConfig.ApiKey

        let region =
            match sendMailConfig.Region with
            | "EU" -> MailGunRegion.EU
            | _ -> MailGunRegion.USA

        services
            .AddFluentEmail(config.FromEmail)
            .AddMailGunSender(domainName, apiKey, region)
            //.AddSendGridSender(sendGridApiKey, sandboxMode)
            .AddHandlebarsRenderer()
        |> ignore

        services.AddSingleton<ISendMailProvider>(SendMailProvider sendMail)
        |> ignore

        Email.DefaultSender <- MailgunSender(domainName, apiKey, region)

        Email.DefaultRenderer <- HandlebarsRenderer()
