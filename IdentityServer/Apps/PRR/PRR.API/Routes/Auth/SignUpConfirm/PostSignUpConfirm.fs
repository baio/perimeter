namespace PRR.API.Routes.Auth.SignUpConfirm

open Giraffe
open Common.Domain.Giraffe
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.API.Routes
open PRR.Domain.Auth.SignUpConfirm

module PostSignUpConfirm =

    let private getFCreateTenant ctx =
#if TEST
        let f =
            bindQueryStringField "skipCreateTenant" ctx

        match f with
        | None -> true
        | Some _ -> false
#else
        false
#endif

    let getEnv ctx =
        let kvStorage = getKeyValueStorage ctx
        let logger = getLogger ctx

        let getTokenItemEnv: GetTokenItem.Env =
            { Logger = logger
              KeyValueStorage = kvStorage }

        { DataContext = getDataContext ctx
          Logger = getLogger ctx
          OnSuccess = onSuccess kvStorage
          GetTokenItem = getTokenItem getTokenItemEnv }

    let private handler ctx =
        task {
            let env = getEnv ctx
            let! data = bindJsonAsync<Data> ctx
            return! signUpConfirm env data
        }

    let createRoute () = POST >=> (wrapHandlerOK handler)
