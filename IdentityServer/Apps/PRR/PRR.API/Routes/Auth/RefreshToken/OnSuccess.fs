namespace PRR.API.Routes.Auth.RefreshToken

open System.Threading.Tasks
open DataAvail.KeyValueStorage.Core
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

                let newRefreshTokenItem: RefreshToken.Item =
                    { ClientId = data.ClientId
                      IsPerimeterClient = data.IsPerimeterClient
                      UserId = data.UserId
                      Token = data.RefreshToken
                      ExpiresAt = expiresIn
                      Scopes = data.Scopes
                      SocialType = data.SocialType }

                let! _ =
                    keyValueStorage.AddValue
                        data.RefreshToken
                        newRefreshTokenItem
                        (Some
                            { PartitionName = REFRESH_TOKEN_KV_PARTITION_NAME
                              ExpiresAt = (Some expiresIn)
                              Tag = (data.UserId.ToString()) })

                ()
            }
