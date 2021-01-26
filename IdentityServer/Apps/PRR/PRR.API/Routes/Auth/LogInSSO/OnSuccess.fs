namespace PRR.API.Routes.Auth.LogInSSO

open System.Threading.Tasks
open PRR.System.Models
open PRR.Domain.Auth.LogIn
open Microsoft.Extensions.Logging
open DataAvail.KeyValueStorage.Core
open Common.Domain.Models.Exceptions
open DataAvail.KeyValueStorage.Core
open Microsoft.Extensions.Logging
open PRR.API.Routes.Auth.KVPartitionNames
open PRR.System.Models
open PRR.Domain.Auth.LogInSSO
open FSharp.Control.Tasks.V2.ContextInsensitive


[<AutoOpen>]
module private OnSuccess =

    type Env =
        { KeyValueStorage: IKeyValueStorage
          Logger: ILogger }


    let onSuccess (env: Env): OnSuccess =
        fun item ->
            task {
                let options =
                    { addValueDefaultOptions with
                          ExpiresAt = (Some item.ExpiresAt)
                          PartitionName = LOG_IN_KV_PARTITION_NAME }

                let! result = env.KeyValueStorage.AddValue item.Code item (Some options)

                match result with
                | Result.Error AddValueError.KeyAlreadyExists ->
                    env.Logger.LogError("Code ${code} already exists in LogIn storage", item.Code)
                    return raise (Unexpected')
                | _ -> ()
            }
