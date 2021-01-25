namespace PRR.API.Routes.Auth.ResetPasswordConfirm

open PRR.API.Routes.Auth.KVPartitionNames
open Akkling
open System.Threading.Tasks
open DataAvail.KeyValueStorage
open PRR.System.Models
open PRR.Domain.Auth.ResetPasswordConfirm

[<AutoOpen>]
module private OnSuccess =

    let onSuccess (KeyValueStorage: IKeyValueStorage): OnSuccess =
        fun email ->
            KeyValueStorage.RemoveValuesByTag
                email
                (Some { PartitionName = RESET_PASSWORD_KV_PARTITION_NAME })
