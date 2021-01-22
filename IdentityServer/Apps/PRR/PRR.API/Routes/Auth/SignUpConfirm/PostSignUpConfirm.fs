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

    let getEnv fCreateTenant ctx =
        { DataContext = getDataContext ctx
          Logger = getLogger ctx
          OnSuccess = onSuccess fCreateTenant (getCQRSSystem ctx)
          GetTokenItem = getTokenItem ctx }

    let private handler ctx =
        task {

            let fCreateTenant = getFCreateTenant ctx

            let env = getEnv fCreateTenant ctx

            let! data = bindJsonAsync<Data> ctx

            return! signUpConfirm env data
        }

    let createRoute () = POST >=> (wrapHandlerNoContent handler)
