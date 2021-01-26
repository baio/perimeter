namespace PRR.API.Routes.Auth.SignUpConfirm

open System.Threading.Tasks
open DataAvail.KeyValueStorage
open PRR.API.Configuration
open PRR.API.Routes.Auth
open PRR.System.Models
open PRR.Domain.Auth.SignUpConfirm
open KVPartitionNames

[<AutoOpen>]
module private OnSuccess =

    let onSuccess (keyValueStorage: IKeyValueStorage): OnSuccess =
        fun data -> keyValueStorage.RemoveValuesByTag data.Email (Some { PartitionName = SIGN_UP_KV_PARTITION_NAME })
