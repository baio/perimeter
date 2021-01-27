namespace PRR.Domain.Auth.RefreshToken

open FSharp.Control.Tasks.V2.ContextInsensitive
open DataAvail.KeyValueStorage.Core
open PRR.Domain.Auth.Common.KeyValueModels

[<AutoOpen>]
module private OnSuccess =


    let onSuccess (env: Env) =
        fun oldRefreshToken (data: RefreshTokenKV) ->
            task {
                let! _ = env.KeyValueStorage.RemoveValue<RefreshTokenKV> oldRefreshToken None

                let! _ =
                    env.KeyValueStorage.AddValue
                        data.Token
                        data
                        (Some
                            { PartitionName = null
                              ExpiresAt = (Some data.ExpiresAt)
                              Tag = (data.UserId.ToString()) })

                ()
            }
