namespace PRR.API.Routes.Auth.ResetPassword

open Giraffe
open Common.Domain.Giraffe
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.API.Routes
open PRR.Domain.Auth.ResetPassword

module PostResetPassword =

    let private handler ctx =
        task {
            let env =
                { DataContext = getDataContext ctx
                  OnSuccess = onSuccess (getCQRSSystem ctx)
                  Logger = getLogger ctx }

            let! data = bindJsonAsync ctx

            return! resetPassword env data
        }

    let createRoute () =
        route "/reset-password"
        >=> POST
        >=> (wrapHandlerNoContent handler)
