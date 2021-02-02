namespace PRR.API.Auth.Routes

open Giraffe
open DataAvail.Giraffe.Common
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Domain.Auth.SignUp

module PostSignUp =

    let private handler' ctx =
        task {

            let env =
                { DataContext = getDataContext ctx
                  HashProvider = getHash ctx
                  Logger = getLogger ctx
                  KeyValueStorage = getKeyValueStorage ctx
                  SendMail = getSendMail ctx
                  TokenExpiresIn = (getConfig ctx).Auth.SignUpTokenExpiresIn
                  PasswordSalter = (getPasswordSalter ctx) }

            let! data = bindJsonAsync ctx

            return! signUp env data
        }

    let handler: HttpHandler = wrapHandlerNoContent handler'
