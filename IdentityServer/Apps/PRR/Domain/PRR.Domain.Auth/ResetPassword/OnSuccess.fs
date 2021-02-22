namespace PRR.Domain.Auth.ResetPassword

open DataAvail.KeyValueStorage.Core
open Microsoft.Extensions.Logging
open PRR.Domain.Auth.Common.KeyValueModels
open PRR.Domain.Auth.ResetPassword
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Domain.Auth.Common
open DataAvail.Http.Exceptions

[<AutoOpen>]
module private OnSuccess =

    let onSuccess (env: Env) email queryString token expiredIn =
        task {
            let options =
                { addValueDefaultOptions with
                      Tag = email
                      ExpiresAt = (Some expiredIn) }

            let! result = env.KeyValueStorage.AddValue<ResetPasswordKV> token { Email = email } (Some options)

            match result with
            | Result.Error AddValueError.KeyAlreadyExists ->
                env.Logger.LogError("${token} already exists in storage", token)
                return raise (Unexpected')
            | _ -> ()

            let mailData =
                { Email = email
                  Token = token
                  QueryString = None }

            env.Logger.LogInformation("Send reset password email to ${email}", mailData.Email)

            do! env.SendMail
                    env.Logger
                    { From = "admin"
                      To = email
                      Subject = "welcome"
                      Template = ResetPasswordMail mailData }

            return ()
        }
