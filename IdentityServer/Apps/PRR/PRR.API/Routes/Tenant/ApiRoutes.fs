namespace PRR.API.Routes.Tenant

open Common.Domain.Giraffe
open Common.Utils
open Common.Utils.ReaderTask
open Giraffe
open PRR.API.Routes
open PRR.Domain.Tenant.Apis

[<AutoOpen>]
module private ApiHandlers =

    let private dataContext = getDataContext |> ofReader

    let createHandler domainId =
        wrap
            (create <!> ((fun ctx -> { AccessTokenExpiresIn = (getConfig ctx).Jwt.AccessTokenExpiresIn }) |> ofReader)
             <*> ((doublet domainId) <!> bindJsonAsync<PostLike>) <*> dataContext)

    let updateHandler domainId id =
        wrap (update <!> ((doublet id) <!> (doublet domainId <!> bindJsonAsync<PostLike>)) <*> dataContext)

    let removeHandler (id: int) =
        wrap (remove id <!> dataContext)

    let getOne (id: int) =
        wrap (getOne id <!> dataContext)

module Api =

    let createRoutes() =
        subRoutef "/tenant/domains/%i/apis" (fun domianId ->
            // TODO : Check domain belongs user
            (choose
                [ POST >=> createHandler domianId
                  // TODO : Check api belongs domain
                  PUT >=> routef "/%i" (updateHandler domianId)
                  DELETE >=> routef "/%i" removeHandler
                  GET >=> routef "/%i" getOne ]))
