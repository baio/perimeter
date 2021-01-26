namespace PRR.API.Routes.Auth.Social

open DataAvail.KeyValueStorage
open PRR.Domain.Auth.Social.SocialAuth
open Akkling
open System.Threading.Tasks
open PRR.API.Routes.DIHelpers
open PRR.Sys.Models.Social
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.API.Routes.Auth.KVPartitionNames
open Microsoft.Extensions.Logging
open Common.Domain.Models

[<AutoOpen>]
module private OnSuccess =

    type Env =
        { KeyValueStorage: IKeyValueStorage
          Logger: ILogger }

    let onSuccess (env: Env): OnSuccess =
        fun data ->
        task {
            let expiresIn = System.DateTime.UtcNow.AddMinutes(float data.ExpiresIn)
            let options =
                { addValueDefaultOptions with
                      ExpiresAt = (Some expiresIn)
                      PartitionName = SOCIAL_LOG_IN_KV_PARTITION_NAME }

            let! result = env.KeyValueStorage.AddValue data.Token data (Some options)

            match result with
            | Result.Error AddValueError.KeyAlreadyExists ->
                env.Logger.LogError("Token ${token} already exists in Social storage", data.Token)
                return raise (Unexpected')
            | _ -> ()
        }
