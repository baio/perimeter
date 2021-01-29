namespace PRR.Domain.Auth.LogInToken

open PRR.Domain.Models
open FSharp.Control.Tasks.V2.ContextInsensitive
open Models
open PRR.Data.DataContext
open System
open System.Text.RegularExpressions
open PRR.Domain.Auth
open PRR.Domain.Auth.Common
open Microsoft.Extensions.Logging
open PRR.Domain.Auth.LogInToken
open PRR.Domain.Common

[<AutoOpen>]
module private LogInTokenSuccess =

    let loginTokenSuccess (env: Env) (loginItem: LogInKV) (refreshTokenItem: RefreshTokenKV) isPerimeterClient =
        task {

            let! _ = env.KeyValueStorage.RemoveValue<LogInKV> loginItem.Code None

            let! result =
                env.KeyValueStorage.AddValue
                    refreshTokenItem.Token
                    refreshTokenItem
                    (Some
                        { PartitionName = null
                          ExpiresAt = (Some refreshTokenItem.ExpiresAt)
                          Tag = (loginItem.UserId.ToString()) })

            match result with
            | Result.Ok _ -> env.Logger.LogInformation("Refresh token successfully added to kv storage")
            | Result.Error err ->
                env.Logger.LogInformation
                    ("Add refresh ${token} token to kv storage gives error ${@err}", refreshTokenItem.Token, err)

            let! event =
                getLoginEvent env.DataContext refreshTokenItem.ClientId loginItem.UserId loginItem.Social isPerimeterClient

            do! env.PublishEndpoint.Publish(event)
        }
