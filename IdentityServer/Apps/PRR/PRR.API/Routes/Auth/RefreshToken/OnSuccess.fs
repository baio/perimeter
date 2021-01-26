namespace PRR.API.Routes.Auth.RefreshToken

open Akkling
open System.Threading.Tasks
open DataAvail.KeyValueStorage
open Microsoft.Extensions.Logging
open PRR.System.Models
open PRR.Domain.Auth.RefreshToken
open PRR.API.Routes.Auth.KVPartitionNames
open FSharp.Control.Tasks.V2.ContextInsensitive

[<AutoOpen>]
module private OnSuccess =

    let onSuccess (keyValueStorage: IKeyValueStorage): OnSuccess =
        fun (data, expiresIn) ->
            task {
                let! _ =
                    keyValueStorage.RemoveValue
                        data.OldRefreshToken
                        (Some { PartitionName = REFRESH_TOKEN_KV_PARTITION_NAME })

                let! _ =
                    keyValueStorage.AddValue
                        data.RefreshToken
                        data
                        (Some
                            { PartitionName = REFRESH_TOKEN_KV_PARTITION_NAME
                              ExpiresAt = (Some expiresIn)
                              Tag = null })

                ()
            }
