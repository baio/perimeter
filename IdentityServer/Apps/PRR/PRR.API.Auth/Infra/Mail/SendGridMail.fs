﻿namespace PRR.API.Auth.Infra.Mail


module SendGridMail = ()

    (*
    let createSendMail (apiKey: string): MailSender =
        let client = SendGridClient apiKey

        fun _ mail ->
            let fromEmail =
                EmailAddress(mail.FromEmail, mail.FromName)

            let toEmail = EmailAddress(mail.ToEmail, mail.ToName)

            let msg =
                MailHelper.CreateSingleEmail(fromEmail, toEmail, mail.Subject, null, mail.Html)
#if TEST || E2E
            Task.FromResult(())
#else
            client.SendEmailAsync msg
            |> TaskUtils.map (fun res ->
                printfn "sent mail result %O" res
                ())
#endif
*)