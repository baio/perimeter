﻿namespace PRR.API.Infra

open PRR.API.Infra.Mail
open PRR.System.Models

[<AutoOpen>]
module SendMail =


    let private getConfirmSignupHtml (proj: ProjectEnv) (item: SignUpToken.Item) =
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

    let private getResetPasswordHtml (proj: ProjectEnv) (item: ResetPassword.Item) =
        sprintf """
            <h2>Hello</h2>
            <p>
                This is you <a href="%s/%s?token=%s">link</a> to reset password.
                <br>
                Cheers, %s.
            </p>
        """ proj.BaseUrl proj.ResetPasswordUrl item.Token proj.Name


    let private createSendMail' (env: MailEnv) (prms: SendMailParams) =
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
            { mail with Html = getResetPasswordHtml env.Project item }

    let createSendMail (env: MailEnv) (sender: MailSender) =
        let sendMail: SendMail =
            fun prms -> createSendMail' env prms |> sender
        sendMail
