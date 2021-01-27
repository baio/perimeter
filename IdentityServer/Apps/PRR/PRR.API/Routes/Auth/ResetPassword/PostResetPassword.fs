namespace PRR.API.Routes.Auth.ResetPassword

open Giraffe
open Common.Domain.Giraffe
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.API.Routes
open PRR.Domain.Auth.ResetPassword

module PostResetPassword =

    let private handler ctx =

        let env =
            { DataContext = getDataContext ctx
              KeyValueStorage = getKeyValueStorage ctx
              SendMail = getSendMail ctx
              Logger = getLogger ctx
              HashProvider = getHash ctx
              TokenExpiresIn = (getConfig ctx).Auth.ResetPasswordTokenExpiresIn }

        task {
            let! data = bindJsonAsync ctx
            return! resetPassword env data
        }

    let createRoute () = POST >=> (wrapHandlerNoContent handler)
