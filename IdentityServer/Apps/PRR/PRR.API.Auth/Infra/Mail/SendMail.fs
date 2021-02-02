namespace PRR.API.Auth.Infra

open PRR.API.Auth.Infra.Mail
open PRR.Domain.Auth.Common

[<AutoOpen>]
module SendMail =


    let private getConfirmSignupHtml (proj: ProjectConfig) (item: ConfirmSignUpMailData) =
        sprintf """
            <h2>Hello %s %s</h2>
            <p>
                This is you <a href="%s/%s?token=%s%s">link</a> to activate %s account.
                <br>
                Cheers, %s
            </p>
        """ item.FirstName item.LastName proj.BaseUrl proj.ConfirmSignUpUrl item.Token
            (match item.QueryString with
             | Some x -> (sprintf "&%s" x)
             | None -> "") proj.Name proj.Name

    let private getResetPasswordHtml (proj: ProjectConfig) (item: ResetPasswordMailData) =
        sprintf """
            <h2>Hello</h2>
            <p>
                This is you <a href="%s/%s?token=%s">link</a> to reset password.
                <br>
                Cheers, %s.
            </p>
        """ proj.BaseUrl proj.ResetPasswordUrl item.Token proj.Name


    let private createSendMail' (env: MailSenderConfig) (prms: SendMailParams) =
        let mail: SendMailData =
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
            { mail with
                  Html =
                      getResetPasswordHtml
                          env.Project
                          { Email = item.Email
                            Token = item.Token } }

    let createSendMail (env: MailSenderConfig) (sender: MailSender) =
        let sendMail: SendMail =
            fun prms -> createSendMail' env prms |> sender

        sendMail
