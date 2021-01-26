namespace PRR.Domain.Auth.SignUp

open System.Threading.Tasks
open DataAvail.KeyValueStorage.Core
open Microsoft.Extensions.Logging
open PRR.System.Models
open FSharp.Control.Tasks.V2.ContextInsensitive
open Common.Domain.Models.Exceptions
open PRR.Domain.Auth.Common

[<AutoOpen>]
module private OnSuccess =

    let onSuccess (env: Env): OnSuccess =
        fun data ->

            let options =
                { Tag = data.Email
                  ExpiresAt = (Some data.ExpiredAt)
                  PartitionName = null }

            task {
                let! result = env.KeyValueStorage.AddValue data.Token data (Some options)

                match result with
                | Result.Error AddValueError.KeyAlreadyExists ->
                    env.Logger.LogError("${token} already exists in storage", data.Token)
                    return raise (Unexpected')
                | _ -> ()

                let mailData: ConfirmSignUpMailData =
                    { FirstName = data.FirstName
                      LastName = data.LastName
                      Email = data.Email
                      Token = data.Token
                      QueryString = data.QueryString }

                do! env.SendMail
                        { From = "admin"
                          To = data.Email
                          Subject = "welcome"
                          Template = ConfirmSignUpMail mailData }
            }
