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

    let private addRefreshToken (env: Env) (userId: int) (refreshTokenItem: RefreshTokenKV) =
        task {
            let! result =
                env.KeyValueStorage.AddValue
                    refreshTokenItem.Token
                    refreshTokenItem
                    (Some
                        { PartitionName = null
                          ExpiresAt = (Some refreshTokenItem.ExpiresAt)
                          Tag = (userId.ToString()) })

            match result with
            | Result.Ok _ -> env.Logger.LogInformation("Refresh token successfully added to kv storage")
            | Result.Error err ->
                env.Logger.LogInformation
                    ("Add refresh ${token} token to kv storage gives error ${@err}", refreshTokenItem.Token, err)
        }


    let onLoginTokenSuccess (env: Env)
                            (clientId: string)
                            (grantType: LogInGrantType)
                            (loginItem: Item)
                            (refreshTokenItem: RefreshTokenKV option)
                            isPerimeterClient
                            =
        task {

            match refreshTokenItem with
            | Some refreshTokenItem -> do! addRefreshToken env loginItem.UserId refreshTokenItem
            | None -> ()

            do! match loginItem.Code with
                | Some code ->
                    env.KeyValueStorage.RemoveValue<LogInKV> code None
                    |> TaskUtils.map (fun _ -> ())
                | None -> Task.FromResult()

            let! event = getLoginEvent env.DataContext clientId grantType isPerimeterClient

            do! env.PublishEndpoint.Publish(event)
        }
