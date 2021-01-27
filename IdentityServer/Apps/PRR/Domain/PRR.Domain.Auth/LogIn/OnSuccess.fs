namespace PRR.Domain.Auth.LogIn

open Microsoft.Extensions.Logging
open PRR.Domain.Auth.Common
open DataAvail.KeyValueStorage.Core
open Common.Domain.Models.Exceptions
open FSharp.Control.Tasks.V2.ContextInsensitive

[<AutoOpen>]
module internal OnSuccess =

    type Env =
        { KeyValueStorage: IKeyValueStorage
          Logger: ILogger }

    let private storeItem<'a> env code expiredAt (item: 'a) =
        task {
            let options =
                { addValueDefaultOptions with
                      ExpiresAt = (Some expiredAt) }

            let! result = env.KeyValueStorage.AddValue<'a> code item (Some options)

            match result with
            | Result.Error AddValueError.KeyAlreadyExists ->
                env.Logger.LogError
                    ("Code ${code} already exists in ${storage} storage",
                     code,
                     (getAddValuePartitionName<'a> (Some options)))

                return raise (Unexpected')
            | _ -> ()
        }

    let private storeSSO env (item: SSOKV) =
        storeItem env item.Code item.ExpiresAt item

    let private storeLogIn env (item: LogInKV) =
        storeItem env item.Code item.ExpiresAt item

    let onSuccess (env: Env) =
        fun (loginItem, ssoItem) ->
            task {
                do! storeLogIn env loginItem
                match ssoItem with
                | Some ssoItem -> do! storeSSO env ssoItem
                | _ -> ()
            }
