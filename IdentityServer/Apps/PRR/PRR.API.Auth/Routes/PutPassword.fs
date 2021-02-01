namespace PRR.API.Auth.Routes

open Giraffe
open DataAvail.Giraffe.Common
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Domain.Auth.UpdatePassword

module PutPassword =

    let private handler' ctx =

        let env =
            { DataContext = getDataContext ctx
              PasswordSalter = getPasswordSalter ctx }

        task {
            let! claimId = bindUserClaimId ctx
            let! data = bindValidateJsonAsync validateData ctx
            return! updatePassword env (claimId, data)
        }

    let handler: HttpHandler = wrapHandlerNoContent handler'
