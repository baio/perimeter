namespace PRR.API.Routes.Auth.ResetPassword

open Akka.Util
open Akkling
open System.Threading.Tasks
open Common.Domain.Models.Exceptions
open DataAvail.KeyValueStorage.Core
open Microsoft.Extensions.Logging
open PRR.API.Infra.Mail.Models
open PRR.System.Models
open PRR.Domain.Auth.ResetPassword
open FSharp.Control.Tasks.V2.ContextInsensitive

[<AutoOpen>]
module private OnSuccess =

    type Env =
        { SendMail: SendMail
          KeyValueStorage: IKeyValueStorage
          Logger: ILogger }

    let onSuccess (env: Env): OnSuccess =
        fun data ->
            task {

                let! result = env.KeyValueStorage.AddValue data.Token data data.ExpiredAt (Some data.Email)

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
