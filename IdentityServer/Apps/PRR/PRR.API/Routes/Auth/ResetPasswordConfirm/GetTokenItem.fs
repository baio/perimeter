namespace PRR.API.Routes.Auth.ResetPasswordConfirm

open Common.Domain.Giraffe
open DataAvail.KeyValueStorage
open Microsoft.Extensions.Logging
open PRR.API
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.System.Models
open Microsoft.Extensions.Logging


[<AutoOpen>]
module private GetTokenItem =

    type Env =
        { KeyValueStorage: IKeyValueStorage
          Logger: ILogger }

    let getTokenItem (env: Env) token =
        task {
            let! result = env.KeyValueStorage.GetValue<ResetPassword.Item> token

            return match result with
                   | Ok result -> Some result
                   | Error err ->
                       env.Logger.LogWarning("Get ${token} item fails with ${@err}", token, err)
                       None
        }
