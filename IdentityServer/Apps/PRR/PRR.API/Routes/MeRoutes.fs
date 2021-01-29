module PRR.API.Routes.Me

open PRR.Domain.Models
open DataAvail.Common
open DataAvail.Common.ReaderTask
open Giraffe
open PRR.API.Routes
open PRR.Domain.Auth.UpdatePassword
open PRR.Domain.Tenant.UserDomains
open DataAvail.Giraffe.Common

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
                                          route "/management/domains" >=> GET >=> getDomainsHandler ]
