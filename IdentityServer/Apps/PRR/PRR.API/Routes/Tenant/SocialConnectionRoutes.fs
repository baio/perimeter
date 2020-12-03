namespace PRR.API.Routes.Tenant

open Common.Domain.Giraffe
open Common.Utils
open Common.Utils.ReaderTask
open Giraffe
open PRR.API.Routes
open PRR.Domain.Auth
open PRR.Domain.Tenant.SocialConnections

[<AutoOpen>]
module private SocialConnectionHandlers =


    let createHandler domainId name =
        wrap
            (create
             <!> (triplet domainId name
                  <!> bindValidateJsonAsync validateData)
             <*> getDataContext')

    let updateHandler (domainId: int) (name: string) =
        wrap
            (update
             <!> ((fun data -> ((domainId, name), data))
                  <!> bindValidateJsonAsync validateData)
             <*> getDataContext')

    let deleteHandler (domainId: int) (name: string) =
        wrap (delete (domainId, name) <!> getDataContext')

    let getByDomainIdHandler domainId =
        wrap (getAll domainId <!> getDataContext')

    let getByClientIdHandler clientId =
        wrap (getAllByClientId clientId <!> getDataContext')

    let reorderHandler domainId =
        wrap
            (reorder domainId
             <!> (bindJsonAsync)
             <*> getDataContext')

    module SocialConnections =

        let createRoutes () =

            choose [ routef "/tenant/apps/%s/social" (fun clientId -> GET >=> getByClientIdHandler clientId)
                     routef "/tenant/domains/%i/social" (fun domainId ->
                         permissionGuard MANAGE_DOMAIN
                         >=> GET
                         >=> getByDomainIdHandler domainId)
                     routef "/tenant/domains/%i/social/order" (fun domainId ->
                         permissionGuard MANAGE_DOMAIN
                         >=> PUT
                         >=> reorderHandler domainId)
                     routef "/tenant/domains/%i/social/%s" (fun (domainId, name) ->
                         permissionGuard MANAGE_DOMAIN
                         >=> choose [ POST >=> (createHandler domainId name)
                                      PUT >=> (updateHandler domainId name)
                                      DELETE >=> (deleteHandler domainId name) ]) ]
