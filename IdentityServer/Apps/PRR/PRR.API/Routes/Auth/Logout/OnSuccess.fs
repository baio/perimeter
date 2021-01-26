namespace PRR.API.Routes.Auth.LogOut

open Common.Domain.Giraffe
open DataAvail.KeyValueStorage
open PRR.API
open PRR.System.Models
open PRR.Domain.Auth.LogOut
open System.Threading.Tasks
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.API.Routes.Auth.KVPartitionNames

[<AutoOpen>]
module private OnSuccess =

    let onSuccess (keyValueStorage: IKeyValueStorage): OnSuccess =
        fun userId ->
            task {
                let! _ =
                    keyValueStorage.RemoveValuesByTag
                        (userId.ToString())
                        (Some { PartitionName = SSO_KV_PARTITION_NAME })

                let! _ =
                    keyValueStorage.RemoveValuesByTag
                        (userId.ToString())
                        (Some { PartitionName = REFRESH_TOKEN_KV_PARTITION_NAME })

                ()
            }
