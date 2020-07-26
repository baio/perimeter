namespace PRR.System

open Akkling
open PRR.System.Models
open System

[<AutoOpen>]
module private SendResetPasswordEmail =
    let sendResetPasswordEmail (sendMail: SendMail) (data: ResetPassword.Item) =

        { From = "admin"
          To = data.Email
          Subject = "welcome"
          Template = ResetPasswordMail data }
        |> sendMail
