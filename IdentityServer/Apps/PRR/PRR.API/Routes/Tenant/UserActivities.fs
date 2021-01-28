namespace PRR.API.Routes.Tenant

open Common.Domain.Models
open Giraffe
open PRR.API.Routes
open PRR.Domain.Auth.GetAudience
open PRR.Domain.Tenant.Views.LogInView
open FSharp.Control.Tasks.V2.ContextInsensitive
open DataAvail.ListQuery.Core

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
        task {
            let db = getViewsDb ctx
            let lq = bindListQuery ctx
            let! result = getLogInList db (domainId, isManagement, lq)
            return! json result next ctx
        }

open UserActivitiesRoutesHandlers

module UsersActivities =

    let createRoutes () =
        choose [ GET
                 >=> routef "/tenant/domains/%i/user-activities" (fun domainId ->
                         Common.Domain.Giraffe.Auth.permissionGuard MANAGE_DOMAIN
                         >=> wrapAudienceGuard fromDomainId domainId
                         >=> getLogIns false domainId)
                 GET
                 >=> routef "/tenant/domains/%i/admin-activities" (fun domainId ->
                         Common.Domain.Giraffe.Auth.permissionGuard MANAGE_DOMAIN
                         >=> wrapAudienceGuard fromDomainId domainId
                         >=> getLogIns true domainId) ]
