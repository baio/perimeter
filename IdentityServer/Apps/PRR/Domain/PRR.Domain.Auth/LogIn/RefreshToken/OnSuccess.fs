namespace PRR.Domain.Auth.LogIn.RefreshToken

open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Domain.Auth.Common

[<AutoOpen>]
module private OnSuccess =

    let onSuccess (env: Env) issuer =
        fun oldRefreshToken (data: RefreshTokenKV) ->

            task {
                let! _ = env.KeyValueStorage.RemoveValue<RefreshTokenKV> oldRefreshToken None
                let tag = getAuthTag issuer data.UserId

                let! _ =
                    env.KeyValueStorage.AddValue
                        data.Token
                        data
                        (Some
                            { PartitionName = null
                              ExpiresAt = (Some data.ExpiresAt)
                              Tag = tag })

                ()
            }
