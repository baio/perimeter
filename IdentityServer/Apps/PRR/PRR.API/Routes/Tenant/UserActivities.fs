namespace PRR.API.Routes.Tenant

open Common.Domain.Giraffe
open Common.Domain.Models
open Common.Utils
open Common.Utils.ReaderTask
open Giraffe
open PRR.API.Routes
open PRR.Domain.Auth.GetAudience

// open PRR.System.Views.LogInView


module private UserActivitiesRoutesHandlers = ()

    // TODO : Restore
    
    (*
    let bindListQuery =
        bindListQuery
            ((function
             | "dateTime" -> Some SortField.DateTime
             | _ -> None),
             (function
             | "email" -> Some FilterField.Email
             | "appIdentifier" -> Some FilterField.AppIdentifier
             | "dateTime" -> Some FilterField.DateTime
             | _ -> None))
        |> ofReader

    let getLogIns (isManagement: bool) (domainId: DomainId) =
        wrap
            (getList
             <!> (ofReader getViewsReaderDb)
             <*> (triplet domainId isManagement <!> bindListQuery))

open UserActivitiesRoutesHandlers

module UsersActivities =

    let createRoutes () =
        choose [ GET
                 >=> routef "/tenant/domains/%i/user-activities" (fun domainId ->
                         permissionGuard MANAGE_DOMAIN
                         >=> wrapAudienceGuard fromDomainId domainId
                         >=> getLogIns false domainId)
                 GET
                 >=> routef "/tenant/domains/%i/admin-activities" (fun domainId ->
                         permissionGuard MANAGE_DOMAIN
                         >=> wrapAudienceGuard fromDomainId domainId
                         >=> getLogIns true domainId) ]
*)