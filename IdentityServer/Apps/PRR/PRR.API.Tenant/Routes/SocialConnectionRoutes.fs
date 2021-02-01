namespace PRR.API.Tenant.Routes

open Giraffe
open PRR.Domain.Tenant.SocialConnections
open DataAvail.Giraffe.Common
open DataAvail.Common
open DataAvail.Common.ReaderTask

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

    let getAllHandler domainId =
        wrap (getAll domainId <!> getDataContext')

    let reorderHandler domainId =
        wrap
            (reorder domainId
             <!> (bindJsonAsync)
             <*> getDataContext')

    module SocialConnections =

        let createRoutes () =

            choose [ routef "/domains/%i/social" (fun domainId ->
                         permissionGuard MANAGE_DOMAIN
                         >=> GET
                         >=> getAllHandler domainId)
                     routef "/domains/%i/social/order" (fun domainId ->
                         permissionGuard MANAGE_DOMAIN
                         >=> PUT
                         >=> reorderHandler domainId)
                     routef "/domains/%i/social/%s" (fun (domainId, name) ->
                         permissionGuard MANAGE_DOMAIN
                         >=> choose [ POST >=> (createHandler domainId name)
                                      PUT >=> (updateHandler domainId name)
                                      DELETE >=> (deleteHandler domainId name) ]) ]
