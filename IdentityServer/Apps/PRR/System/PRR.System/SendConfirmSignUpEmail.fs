namespace PRR.System

open Akkling
open PRR.System.Models
open System

[<AutoOpen>]
module private SendConfirmSignUpEmail =
    let sendConfirmSignUpEmail (sendMail: SendMail) (data: SignUpToken.Item) =

        { From = "admin"
          To = data.Email
          Subject = "welcome"
          Template = ConfirmSignUpMail data }
        |> sendMail
