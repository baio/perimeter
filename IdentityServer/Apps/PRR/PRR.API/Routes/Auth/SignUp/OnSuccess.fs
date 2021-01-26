namespace PRR.API.Routes.Auth.SignUp

open Akkling
open System.Threading.Tasks
open DataAvail.KeyValueStorage
open Microsoft.Extensions.Logging
open PRR.API.Routes.Auth
open PRR.System.Models
open PRR.Domain.Auth.SignUp
open KVPartitionNames
open FSharp.Control.Tasks.V2.ContextInsensitive
open Common.Domain.Models.Exceptions

[<AutoOpen>]
module private OnSuccess =

    type Env =
        { SendMail: SendMail
          KeyValueStorage: IKeyValueStorage
          Logger: ILogger }

    let onSuccess (env: Env): OnSuccess =
        fun (data, expireAt) ->
            let options =
                { Tag = data.Email
                  ExpiresAt = (Some expireAt)
                  PartitionName = SIGN_UP_KV_PARTITION_NAME }

            let signUpTokenItem: SignUpToken.Item =
                { FirstName = data.FirstName
                  LastName = data.LastName
                  Email = data.Email
                  Password = data.Password
                  Token = data.Token
                  ExpiredAt = expireAt
                  QueryString = data.QueryString }
            
            task {
                let! result = env.KeyValueStorage.AddValue data.Token signUpTokenItem (Some options)

                match result with
                | Result.Error AddValueError.KeyAlreadyExists ->
                    env.Logger.LogError("${token} already exists in storage", data.Token)
                    return raise (Unexpected')
                | _ -> ()


                do! env.SendMail
                        { From = "admin"
                          To = data.Email
                          Subject = "welcome"
                          Template = ConfirmSignUpMail signUpTokenItem }
            }
