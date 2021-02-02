namespace PRR.Domain.Auth

open PRR.Domain.Models
open DataAvail.Common
open PRR.Data.DataContext
open System.Linq
open DataAvail.EntityFramework.Common

[<AutoOpen>]
module ApplicationInfo =

    [<CLIMutable>]
    type AppInfo =
        { Title: string
          SocialConnections: string seq }

    let private getCommon (clientId: ClientId) (dataContext: DbDataContext) =
        query {
            for app in dataContext.Applications do
                where (app.ClientId = clientId)
                select
                    { Title = app.Name
                      SocialConnections =
                          app.Domain.SocialConnections.OrderBy(fun x -> x.Order).Select(fun x -> x.SocialName) }
        }
        |> toSingleAsync


    let private getPerimeter () =
        { Title = "Perimeter"
          SocialConnections =
              [ (socialType2Name SocialType.Github)
                (socialType2Name SocialType.Google) ] }

    type ApplicationInfoEnv =
        { DataContext: DbDataContext
          PerimeterSocialProviders: PerimeterSocialProviders }

    let getApplicationInfo (clientId: ClientId) (dataContext: DbDataContext) =
        if clientId = DEFAULT_CLIENT_ID then getPerimeter () |> TaskUtils.returnM else getCommon clientId dataContext
