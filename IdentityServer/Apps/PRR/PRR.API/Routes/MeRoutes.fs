module PRR.API.Routes.Me

open Common.Domain.Giraffe
open Common.Domain.Models
open Common.Utils
open Common.Utils.ReaderTask
open Giraffe
open PRR.API.Routes
open PRR.Domain.Auth.UpdatePassword
open PRR.Domain.Tenant.UserDomains

module private Handlers =

    let updatePasswordHandler =
        wrap
            (updatePassword <!> ((fun ctx ->
                                 { DataContext = getDataContext ctx
                                   PasswordSalter = getPasswordSalter ctx })
                                 |> ofReader)
             <*> (doublet <!> bindUserClaimId <*> bindValidateJsonAsync validateData))

    let getDomainsHandler =
        wrap (getClientDomains <!> getDataContext' <*> bindUserClaimId)

open Handlers

let createRoutes() =
    subRoute "/me" requiresAuth >=> choose
                                        [ route "/password" >=> PUT >=> updatePasswordHandler
                                          route "/domains" >=> GET >=> getDomainsHandler ]
