namespace PRR.Domain.Tenant

open Common.Domain.Models
open PRR.Data.DataContext
open Common.Domain.Utils

[<AutoOpen>]
module ApplicationInfo =

    [<CLIMutable>]
    type AppInfo = { Title: string }

    let getApplicationInfo (dataContext: DbDataContext) (clientId: ClientId) =
        query {
            for app in dataContext.Applications do
                where (app.ClientId = clientId)
                select { Title = app.Name }
        }
        |> toSingleExnAsync NotFound
