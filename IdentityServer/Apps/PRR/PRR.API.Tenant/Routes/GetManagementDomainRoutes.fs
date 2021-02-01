namespace PRR.API.Routes.Tenant

open Giraffe
open PRR.API.Routes
open DataAvail.Common.ReaderTask
open PRR.Domain.Tenant.UserDomains

open DataAvail.Giraffe.Common


module GetManagementDomainRoutes =

    let private getDomainsHandler =
        wrap
            (getClientDomains <!> getDataContext'
             <*> bindUserClaimId)


    let createRoutes () =
        route "/tenant/management/domains"
        >=> GET
        >=> getDomainsHandler
