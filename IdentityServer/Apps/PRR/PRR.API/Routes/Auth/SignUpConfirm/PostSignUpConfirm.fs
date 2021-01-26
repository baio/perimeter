namespace PRR.API.Routes.Auth.SignUpConfirm

open Giraffe
open Common.Domain.Giraffe
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.API.Routes
open PRR.Domain.Auth.SignUpConfirm

module PostSignUpConfirm =

    let getEnv ctx =

        { DataContext = getDataContext ctx
          Logger = getLogger ctx
          KeyValueStorage = getKeyValueStorage ctx }

    let private handler ctx =
        task {
            let env = getEnv ctx
            let! data = bindJsonAsync<Data> ctx
            return! signUpConfirm env data
        }

    let createRoute () = POST >=> (wrapHandlerOK handler)
