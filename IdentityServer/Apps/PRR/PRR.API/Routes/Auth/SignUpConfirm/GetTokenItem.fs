namespace PRR.API.Routes.Auth.SignUpConfirm

open Common.Domain.Giraffe
open PRR.API
open PRR.System.Models
open PRR.API.Routes.Auth.KVPartitionNames
open Common.Domain.Giraffe
open DataAvail.KeyValueStorage
open Microsoft.Extensions.Logging
open PRR.API
open FSharp.Control.Tasks.V2.ContextInsensitive

[<AutoOpen>]
module private GetTokenItem =

    type Env =
        { KeyValueStorage: IKeyValueStorage
          Logger: ILogger }

    let getTokenItem (env: Env) token =
        task {
            let! result =
                env.KeyValueStorage.GetValue<SignUpToken.Item>
                    token
                    (Some { PartitionName = SIGN_UP_KV_PARTITION_NAME })

            return match result with
                   | Ok result -> Some result
                   | Error err ->
                       env.Logger.LogWarning("Get ${token} item fails with ${@err}", token, err)
                       None
        }
