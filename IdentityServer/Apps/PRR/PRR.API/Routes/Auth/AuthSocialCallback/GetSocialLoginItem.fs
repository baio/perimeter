namespace PRR.API.Routes.Auth.SocialCallback

open Common.Domain.Models
open PRR.API.Routes.Auth.KVPartitionNames
open DataAvail.KeyValueStorage.Core
open Microsoft.Extensions.Logging
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Sys.Models.Social

[<AutoOpen>]
module private GetSocialLoginItem =

    type Env =
        { KeyValueStorage: IKeyValueStorage
          Logger: ILogger }

    let getSocialLoginItem (env: Env) (token: Token) =
        task {
            let! result =
                env.KeyValueStorage.GetValue<Item> token (Some { PartitionName = SOCIAL_LOG_IN_KV_PARTITION_NAME })

            return match result with
                   | Ok result -> Some result
                   | Error err ->
                       env.Logger.LogWarning("Get SocialLoginItem ${token} item fails with ${@err}", token, err)
                       None
        }
