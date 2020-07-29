namespace PRR.API.Infra

open Common.Utils
open PRR.System.Models
open SendGrid
open SendGrid.Helpers.Mail
open System
open System.Threading.Tasks

[<AutoOpen>]
module SendMail =

    [<CLIMutable>]
    type ProjectEnv =
        { Name: string
          BaseUrl: string
          ConfirmSignUpUrl: string
          ResetPasswordUrl: string }

    [<CLIMutable>]
    type MailEnv =
        { ApiKey: string
          Project: ProjectEnv
          FromEmail: string
          FromName: string }

    type private SendGridMail =
        { FromEmail: string
          FromName: string
          ToEmail: string
          ToName: string
          Subject: string
          Html: string }

    let private sendMailContent (client: SendGridClient) (mail: SendGridMail) =
        let fromEmail = EmailAddress(mail.FromEmail, mail.FromName)
        let toEmail = EmailAddress(mail.ToEmail, mail.ToName)
        let msg = MailHelper.CreateSingleEmail(fromEmail, toEmail, mail.Subject, null, mail.Html)
#if TEST
        Task.FromResult(())
#else
        client.SendEmailAsync msg |> TaskUtils.map (fun res ->
            sprintf "sent mail result %O" res
            ())
#endif


    let private getConfirmSignupHtml (proj: ProjectEnv) (item: SignUpToken.Item) =
        sprintf """
            <h2>Hello %s %s</h2>
            <p>
                This is you <a href="%s/%s?token=%s">link</a> to activate %s account.
                Cheers, %s
            </p>
        """ item.FirstName item.LastName proj.BaseUrl proj.ConfirmSignUpUrl item.Token proj.Name proj.Name

    let private getResetPasswordHtml (proj: ProjectEnv) (item: ResetPassword.Item) =
        sprintf """
            <h2>Hello</h2>
            <p>
                This is you <a href="%s/%s?token=%s">link</a> to reset password.
                Cheers, %s.
            </p>
        """ proj.BaseUrl proj.ConfirmSignUpUrl item.Token proj.Name


    let private createSendMail (env: MailEnv) (prms: SendMailParams) =
        let mail: SendGridMail =
            { FromEmail = env.FromEmail
              FromName = env.FromName
              ToEmail = prms.To
              ToName = prms.To
              Subject = prms.Subject
              Html = "" }
        match prms.Template with
        | ConfirmSignUpMail item ->
            { mail with
                  ToName = (sprintf "%s %s" item.FirstName item.LastName)
                  Html = getConfirmSignupHtml env.Project item }
        | ResetPasswordMail item ->
            { mail with Html = getResetPasswordHtml env.Project item }

    let createMailSender (env: MailEnv) =
        let client = SendGridClient env.ApiKey
        let sendMail: SendMail =
            fun prms ->
                createSendMail env prms |> sendMailContent client
        sendMail
