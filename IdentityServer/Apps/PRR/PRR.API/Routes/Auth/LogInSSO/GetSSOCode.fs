namespace PRR.API.Routes.Auth.LogInSSO

open PRR.API.Routes.Auth.KVPartitionNames
open DataAvail.KeyValueStorage.Core
open Microsoft.Extensions.Logging
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.System.Models

[<AutoOpen>]
module private GetSSOCode =

    type Env =
        { KeyValueStorage: IKeyValueStorage
          Logger: ILogger }

    let getSSOCode (env: Env) sso =
        task {
            let! result = env.KeyValueStorage.GetValue<SSO.Item> sso (Some { PartitionName = SSO_KV_PARTITION_NAME })

            return match result with
                   | Ok result -> Some result
                   | Error err ->
                       env.Logger.LogWarning("Get sso ${@sso} item fails with ${@err}", sso, err)
                       None
        }
