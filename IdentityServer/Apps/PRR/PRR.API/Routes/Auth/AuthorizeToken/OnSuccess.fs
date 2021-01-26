namespace PRR.API.Routes.Auth.AuthorizeToken

open System.Threading.Tasks
open DataAvail.KeyValueStorage
open Microsoft.Extensions.Logging
open PRR.System.Models
open PRR.Domain.Auth.LogInToken
open PRR.API.Routes.Auth.KVPartitionNames
open FSharp.Control.Tasks.V2.ContextInsensitive

[<AutoOpen>]
module private OnSuccess =

    let onSuccess (keyValueStorage: IKeyValueStorage): OnSuccess =
        fun (token, item, _) ->
            task {
                let! _ = keyValueStorage.RemoveValue token (Some { PartitionName = LOG_IN_KV_PARTITION_NAME })

                let! _ =
                    keyValueStorage.AddValue
                        item.Token
                        item
                        (Some
                            { PartitionName = REFRESH_TOKEN_KV_PARTITION_NAME
                              ExpiresAt = (Some item.ExpiresAt)
                              Tag = (item.UserId.ToString()) })

                ()
            }
