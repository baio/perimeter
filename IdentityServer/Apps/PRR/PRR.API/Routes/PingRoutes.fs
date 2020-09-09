namespace PRR.API.Routes

module VersionRoutes =

    let private recreatedDb ctx =
        let dataContext = getDataContext ctx
        dataContext.Database.EnsureDeleted() |> ignore
        dataContext.Database.Migrate() |> ignore

    let createRoutes () =
        choose [ route "/e2e/reset"
