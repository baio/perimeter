namespace PRR.API.Routes

open Giraffe
open Microsoft.EntityFrameworkCore

module E2E =

    let createRoutes() =

        route "/e2e/reset" >=> POST >=> fun next ctx ->
            // Recreate db on start
            let dataContext = getDataContext ctx
            dataContext.Database.EnsureDeleted() |> ignore
            dataContext.Database.Migrate() |> ignore
            Successful.NO_CONTENT next ctx
