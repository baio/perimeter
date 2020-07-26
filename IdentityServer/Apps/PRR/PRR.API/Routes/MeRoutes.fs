module PRR.API.Routes.Me

open Common.Domain.Giraffe
open Common.Utils
open Common.Utils.ReaderTask
open Giraffe
open PRR.API.Routes
open PRR.Domain.Auth.UpdatePassword

module private Handlers =

    let updatePasswordHandler =
        wrap
            (updatePassword <!> ((fun ctx ->
                                 { DataContext = getDataContext ctx
                                   PasswordSalter = getPasswordSalter ctx })
                                 |> ofReader)
             <*> (doublet <!> bindUserClaimId <*> bindJsonAsync<Data>))


open Handlers

let createRoutes() =
    route "/me/password" >=> PUT >=> updatePasswordHandler
