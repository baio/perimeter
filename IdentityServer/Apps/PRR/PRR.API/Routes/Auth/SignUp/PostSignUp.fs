namespace PRR.API.Routes.Auth.SignUp

open Giraffe
open Common.Domain.Giraffe
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.API.Routes
open PRR.Domain.Auth.SignUp

module PostSignUp =

    let private handler ctx =
        task {
            let env =
                { DataContext = getDataContext ctx
                  HashProvider = getHash ctx
                  Logger = ctx.GetLogger()
                  OnSuccess = onSuccess (getCQRSSystem ctx) }

            let! data = bindJsonAsync ctx

            return! signUp env data
        }

    let createRoute () =
        route "/sign-up"
        >=> POST
        >=> (wrapHandlerNoContent handler)
