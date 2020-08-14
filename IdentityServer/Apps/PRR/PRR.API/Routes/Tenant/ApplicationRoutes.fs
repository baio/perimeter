namespace PRR.API.Routes.Tenant

open Common.Domain.Giraffe
open Common.Utils
open Common.Utils.ReaderTask
open Giraffe
open PRR.API.Infra
open PRR.API.Routes
open PRR.Domain.Tenant.Applications

[<AutoOpen>]
module private ApplicationHandlers =

    let private dataContext = getDataContext |> ofReader

    let createHandler domainId =
        wrap
            (create <!> ((fun ctx ->
                         let config = getConfig ctx
                         { RefreshTokenExpiresIn = config.Jwt.RefreshTokenExpiresIn
                           IdTokenExpiresIn = config.Jwt.IdTokenExpiresIn
                           HashProvider = ctx |> getHash })
                         |> ofReader) <*> ((doublet domainId) <!> bindJsonAsync<PostLike>) <*> dataContext)

    let updateHandler domainId id =
        wrap (update <!> ((doublet id) <!> (doublet domainId <!> bindJsonAsync<PostLike>)) <*> dataContext)

    let removeHandler (id: int) =
        wrap (remove id <!> dataContext)

    let getOne (id: int) =
        wrap (getOne id <!> dataContext)

    let bindListQuery =
        bindListQuery
            ((function
             | "name" ->
                 Some SortField.Name
             | "dateCreated" ->
                 Some SortField.DateCreated
             | _ -> None),
             (function
             | "name" ->
                 Some FilterField.Name
             | _ -> None))
        |> ofReader

    let getList domainId =
        wrap (getList <!> getDataContext' <*> ((doublet domainId) <!> bindListQuery))

module Application =

    let createRoutes() =
        subRoutef "/tenant/domains/%i/applications" (fun domainId ->
            // TODO : Check domain belongs user
            (choose
                [ POST >=> createHandler domainId
                  // TODO : Check app belongs domain
                  PUT >=> routef "/%i" (updateHandler domainId)
                  DELETE >=> routef "/%i" removeHandler
                  GET >=> getList domainId
                  GET >=> routef "/%i" getOne ]))
