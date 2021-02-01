namespace PRR.API.Auth.Routes

open Giraffe
open DataAvail.Giraffe.Common
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Domain.Auth.ResetPassword

module PostResetPassword =

    let private handler' ctx =

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

    let handler: HttpHandler = wrapHandlerNoContent handler'
