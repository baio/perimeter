namespace PRR.API.Auth.Routes

open Giraffe
open DataAvail.Giraffe.Common
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Domain.Auth.SignUpConfirm

module PostSignUpConfirm =

    let getEnv ctx =

        { DataContext = getDataContext ctx
          Logger = getLogger ctx
          KeyValueStorage = getKeyValueStorage ctx }

    let private handler' ctx =
        task {
            let env = getEnv ctx
            let! data = bindJsonAsync<Data> ctx
            return! signUpConfirm env data
        }

    let handler: HttpHandler = wrapHandlerOK handler'
