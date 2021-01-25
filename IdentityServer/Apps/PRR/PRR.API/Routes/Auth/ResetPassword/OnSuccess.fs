namespace PRR.API.Routes.Auth.ResetPassword

open Akka.Util
open Akkling
open System.Threading.Tasks
open Common.Domain.Models.Exceptions
open DataAvail.KeyValueStorage.Core
open Microsoft.Extensions.Logging
open PRR.API.Infra.Mail.Models
open PRR.API.Routes.Auth.KVPartitionNames
open PRR.System.Models
open PRR.Domain.Auth.ResetPassword
open FSharp.Control.Tasks.V2.ContextInsensitive
open DataAvail.KeyValueStorage.Core

[<AutoOpen>]
module private OnSuccess =

    type Env =
        { SendMail: SendMail
          KeyValueStorage: IKeyValueStorage
          Logger: ILogger }

    let onSuccess (env: Env): OnSuccess =
        fun data ->
            task {

                let options =
                    { addValueDefaultOptions with
                          Tag = data.Email
                          ExpiresAt = (Some data.ExpiredAt)
                          PartitionName = RESET_PASSWORD_KV_PARTITION_NAME }

                let! result = env.KeyValueStorage.AddValue data.Token data.Email (Some options)

                match result with
                | Result.Error AddValueError.KeyAlreadyExists ->
                    env.Logger.LogError("${token} already exists in storage", data.Token)
                    return raise (Unexpected')
                | _ -> ()

                { From = "admin"
                  To = data.Email
                  Subject = "welcome"
                  Template = ResetPasswordMail data }
                |> env.SendMail

                return ()
            }
// sys.EventsRef <! (ResetPasswordRequested email)
