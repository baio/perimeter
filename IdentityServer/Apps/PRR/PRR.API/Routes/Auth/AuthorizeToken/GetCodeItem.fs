namespace PRR.API.Routes.Auth.AuthorizeToken

open PRR.API.Routes.Auth.KVPartitionNames
open DataAvail.KeyValueStorage
open Microsoft.Extensions.Logging
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.System.Models

[<AutoOpen>]
module private GetCodeItem =

    type Env =
        { KeyValueStorage: IKeyValueStorage
          Logger: ILogger }

    let getCodeItem (env: Env) token =
        task {
            let! result = env.KeyValueStorage.GetValue<LogIn.Item> token (Some { PartitionName = LOG_IN_KV_PARTITION_NAME })

            return match result with
                   | Ok result -> Some result
                   | Error err ->
                       env.Logger.LogWarning("Get LogIn ${token} item fails with ${@err}", token, err)
                       None
        }
