namespace PRR.API.Routes.Auth.ResetPasswordConfirm

open PRR.API.Routes.Auth.KVPartitionNames
open Common.Domain.Giraffe
open DataAvail.KeyValueStorage
open Microsoft.Extensions.Logging
open PRR.API
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.System.Models
open Microsoft.Extensions.Logging


[<AutoOpen>]
module private GetTokenItem =

    type Env =
        { KeyValueStorage: IKeyValueStorage
          Logger: ILogger }

    let getTokenItem (env: Env) token =
        task {
            let! result =
                env.KeyValueStorage.GetValue<string>
                    token
                    (Some { PartitionName = RESET_PASSWORD_KV_PARTITION_NAME })

            return match result with
                   | Ok result -> Some result
                   | Error err ->
                       env.Logger.LogWarning("Get ${token} item fails with ${@err}", token, err)
                       None
        }
