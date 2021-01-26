namespace PRR.API.Routes.Auth.RefreshToken

open PRR.API.Routes.Auth.KVPartitionNames
open DataAvail.KeyValueStorage
open Microsoft.Extensions.Logging
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.System.Models

[<AutoOpen>]
module private GetTokenItem =

    type Env =
        { KeyValueStorage: IKeyValueStorage
          Logger: ILogger }

    let getTokenItem (env: Env) token =
        task {
            let! result =
                env.KeyValueStorage.GetValue<RefreshToken.Item>
                    token
                    (Some { PartitionName = REFRESH_TOKEN_KV_PARTITION_NAME })

            return match result with
                   | Ok result -> Some result
                   | Error err ->
                       env.Logger.LogWarning("Get refresh token ${token} item fails with ${@err}", token, err)
                       None
        }
