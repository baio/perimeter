namespace PRR.API.Routes.Auth.ResetPasswordConfirm

open Giraffe
open Common.Domain.Giraffe
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.API.Routes
open PRR.Domain.Auth.ResetPasswordConfirm
open Microsoft.Extensions.Logging


module PostResetPasswordConfirm =


    let private handler ctx =
        task {

            let logger = getLogger ctx
            let kvStorage = getKeyValueStorage ctx

            let getTokenIemEnv: GetTokenItem.Env =
                { Logger = logger
                  KeyValueStorage = kvStorage }

            let env =
                { DataContext = getDataContext ctx
                  Logger = logger
                  OnSuccess = onSuccess kvStorage
                  GetTokenItem = getTokenItem getTokenIemEnv
                  PasswordSalter = getPasswordSalter ctx }

            let! data = bindJsonAsync ctx

            return! resetPasswordConfirm env data
        }

    let createRoute () = POST >=> (wrapHandlerNoContent handler)
