namespace PRR.API.Tenant.Routes

open Giraffe
open DataAvail.Common.ReaderTask
open PRR.Domain.Tenant.UserDomains
open DataAvail.Giraffe.Common

module GetManagementDomainRoutes =

    let private getDomainsHandler =
        wrap
            (getClientDomains <!> getDataContext'
             <*> bindUserClaimId)


    let createRoutes () =
        route "/management/domains"
        >=> GET
        >=> getDomainsHandler
