namespace PRR.API.Routes.Auth.LogIn

open Akka.Configuration.Hocon
open Akkling
open System.Threading.Tasks
open PRR.System.Models
open PRR.Domain.Auth.LogIn
open Microsoft.Extensions.Logging
open DataAvail.KeyValueStorage.Core
open Common.Domain.Models.Exceptions
open DataAvail.KeyValueStorage.Core
open Microsoft.Extensions.Logging
open PRR.API.Routes.Auth.KVPartitionNames
open PRR.System.Models
open PRR.Domain.Auth.LogIn
open FSharp.Control.Tasks.V2.ContextInsensitive

[<AutoOpen>]
module internal OnSuccess =

    type Env =
        { KeyValueStorage: IKeyValueStorage
          Logger: ILogger }

    let private storeLogIn env (item: LogIn.Item) =
        task {
            let options =
                { addValueDefaultOptions with
                      ExpiresAt = (Some item.ExpiresAt)
                      PartitionName = LOG_IN_KV_PARTITION_NAME }

            let! result = env.KeyValueStorage.AddValue item.Code item (Some options)

            match result with
            | Result.Error AddValueError.KeyAlreadyExists ->
                env.Logger.LogError("Code ${code} already exists in LogIn storage", item.Code)
                return raise (Unexpected')
            | _ -> ()
        }

    let private storeSSO env (item: SSO.Item) =
        task {
            let options =
                { Tag = (item.UserId.ToString())
                  ExpiresAt = (Some item.ExpiresAt)
                  PartitionName = SSO_KV_PARTITION_NAME }

            let! result = env.KeyValueStorage.AddValue item.Code item (Some options)

            match result with
            | Result.Error AddValueError.KeyAlreadyExists ->
                env.Logger.LogError("Code ${code} already exists in SSO storage", item.Code)
                return raise (Unexpected')
            | _ -> ()
        }

    let onSuccess (env: Env): OnSuccess =
        fun (loginItem, ssoItem) ->
            task {

                do! storeLogIn env loginItem
                match ssoItem with
                | Some ssoItem -> do! storeSSO env ssoItem
                | _ -> ()

                return ()
            }
