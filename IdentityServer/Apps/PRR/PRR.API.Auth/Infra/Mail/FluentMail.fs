namespace PRR.API.Auth.Infra.Mail

open System.Threading.Tasks
open DataAvail.Common
open FSharp.Control.Tasks.V2.ContextInsensitive
open FluentEmail.Core
open Microsoft.Extensions.Logging

module FluentMail =

    let createSendMail: MailSender =
        fun logger mail ->
            logger.LogInformation("Start send email ${@mail}", mail)

            let email =
                Email()
                    .SetFrom(mail.FromEmail, mail.FromName)
                    .To(mail.ToEmail, mail.ToName)
                    .Subject(mail.Subject)
                    .UsingTemplate(mail.Html, null, true)

            task {
                try
                    let! result = email.SendAsync()

                    if result.Successful
                    then logger.LogInformation("Send email success result ${@result}", result)
                    else logger.LogError("Send email fails ${@error}", result.ErrorMessages)

                    return ()
                with ex ->
                    logger.LogError("Send email fail ${@ex}", ex)
                    return raise ex
            }
(*
#if TEST || E2E
                        Task.FromResult(())
#else
                task {
                    return emal
                }
#endif
*)
