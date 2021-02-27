namespace PRR.Domain.Auth.LogIn.Common

open System.Threading.Tasks
open DataAvail.Common
open DataAvail.KeyValueStorage.Core.KeyValueStorage
open FSharp.Control.Tasks.V2.ContextInsensitive
open MassTransit
open Microsoft.Extensions.Logging
open PRR.Data.DataContext
open PRR.Domain.Auth.Common
open PRR.Domain.Common.Events
open PRR.Domain.Models.Social

[<AutoOpen>]
module OnLogInTokenSuccess =

    type Env =
        { DataContext: DbDataContext
          KeyValueStorage: IKeyValueStorage
          Logger: ILogger
          PublishEndpoint: IPublishEndpoint }

    type Item =
        { Code: string option
          ClientId: string
          Social: Social option
          UserId: int }

    let onLoginTokenSuccess (env: Env)
                            (grantType: LogInGrantType)
                            (loginItem: Item)
                            (refreshTokenItem: RefreshTokenKV)
                            isPerimeterClient
                            =
        task {

            do! match loginItem.Code with
                | Some code ->
                    env.KeyValueStorage.RemoveValue<LogInKV> code None
                    |> TaskUtils.map (fun _ -> ())
                | None -> Task.FromResult()

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

            let! event = getLoginEvent env.DataContext refreshTokenItem.ClientId grantType isPerimeterClient

            do! env.PublishEndpoint.Publish(event)
        }
