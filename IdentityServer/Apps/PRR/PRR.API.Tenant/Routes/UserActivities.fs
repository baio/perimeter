namespace PRR.API.Tenant.Routes

open PRR.Domain.Models
open Giraffe
open PRR.Domain.Tenant.Views.LogInView
open FSharp.Control.Tasks.V2.ContextInsensitive
open DataAvail.ListQuery.Core
open Microsoft.Extensions.Logging

module private UserActivitiesRoutesHandlers =

    let private bindListQuery =
        bindListQuery
            ((function
             | "dateTime" -> Some SortField.DateTime
             | _ -> None),
             (function
             | "email" -> Some FilterField.Email
             | "appIdentifier" -> Some FilterField.AppIdentifier
             | "dateTime" -> Some FilterField.DateTime
             | _ -> None))

    let getLogIns (isManagement: bool) (domainId: DomainId) next ctx =
        let logger = getLogger ctx
        logger.LogInformation("getLogIns ${isManagement} ${domainId}", isManagement, domainId)
        task {
            let db = getViewsDb ctx
            let lq = bindListQuery ctx
            logger.LogInformation("ListQuery ${@listQuery}", lq)
            let! result = getLogInList db (domainId, isManagement, lq)
            logger.LogInformation("ListQuery success")
            return! json result next ctx
        }

open UserActivitiesRoutesHandlers

module UsersActivities =

    let createRoutes () =
        choose [ GET
                 >=> routef "/domains/%i/user-activities" (fun domainId ->
                         DataAvail.Giraffe.Common.Auth.permissionGuard MANAGE_DOMAIN
                         >=> wrapAudienceGuard fromDomainId domainId
                         >=> getLogIns false domainId)
                 GET
                 >=> routef "/domains/%i/admin-activities" (fun domainId ->
                         DataAvail.Giraffe.Common.Auth.permissionGuard MANAGE_DOMAIN
                         >=> wrapAudienceGuard fromDomainId domainId
                         >=> getLogIns true domainId) ]
