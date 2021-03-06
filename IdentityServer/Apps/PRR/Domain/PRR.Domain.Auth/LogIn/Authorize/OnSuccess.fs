namespace PRR.Domain.Auth.LogIn.Authorize

open Microsoft.Extensions.Logging
open PRR.Domain.Auth.Common
open DataAvail.KeyValueStorage.Core
open FSharp.Control.Tasks.V2.ContextInsensitive
open DataAvail.Http.Exceptions

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

    let onSuccess (env: Env) =
        fun ((loginItem, ssoItem): (LogInKV * SSOKV option)) ->
            task {
                do! storeItem env loginItem.Code loginItem.ExpiresAt loginItem
                match ssoItem with
                | Some ssoItem ->
                    // Update SSO item expireTime
                    env.Logger.LogDebug("Remove SSO from storage", ssoItem.Code)
                    let! _ = env.KeyValueStorage.RemoveValue<SSOKV> ssoItem.Code None
                    env.Logger.LogDebug("Add SSO to storage", ssoItem.Code)
                    do! storeItem env ssoItem.Code ssoItem.ExpiresAt ssoItem
                | _ -> ()
            }
