﻿namespace PRR.API.Routes.Auth

open Giraffe
open Common.Domain.Giraffe
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.API.Routes
open PRR.Domain.Auth.ResetPasswordConfirm

module PostResetPasswordConfirm =

    let private handler' ctx =
        task {
            let env =
                { DataContext = getDataContext ctx
                  Logger = getLogger ctx
                  KeyValueStorage = getKeyValueStorage ctx
                  PasswordSalter = getPasswordSalter ctx }

            let! data = bindJsonAsync ctx

            return! resetPasswordConfirm env data
        }

    let handler: HttpHandler = wrapHandlerNoContent handler'