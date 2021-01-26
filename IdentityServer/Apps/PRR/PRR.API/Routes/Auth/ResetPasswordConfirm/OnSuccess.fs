namespace PRR.API.Routes.Auth.ResetPasswordConfirm

open PRR.API.Routes.Auth.KVPartitionNames
open System.Threading.Tasks
open DataAvail.KeyValueStorage.Core
open PRR.System.Models
open PRR.Domain.Auth.ResetPasswordConfirm

[<AutoOpen>]
module private OnSuccess =

    let onSuccess (keyValueStorage: IKeyValueStorage): OnSuccess =
        fun email -> keyValueStorage.RemoveValuesByTag email (Some { PartitionName = RESET_PASSWORD_KV_PARTITION_NAME })