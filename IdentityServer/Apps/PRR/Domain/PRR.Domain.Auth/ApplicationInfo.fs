namespace PRR.Domain.Auth

open FSharp.Control.Tasks.V2.ContextInsensitive
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
        task {
            let! (result, isDomainManagement, isTenantManagement) =
                query {
                    for app in dataContext.Applications do
                        where (app.ClientId = clientId)

                        select
                            ({ Title = app.Name
                               SocialConnections =
                                   app
                                       .Domain
                                       .SocialConnections
                                       .OrderBy(fun x -> x.Order)
                                       .Select(fun x -> x.SocialName) },
                             app.IsDomainManagement,
                             app.Domain.Pool = null)
                }
                |> toSingleAsync

            let isManagement = isDomainManagement || isTenantManagement
            return (isManagement, result)
        }


    let private getPerimeter () =
        { Title = "Perimeter"
          SocialConnections =
              [ (socialType2Name SocialType.Github)
                (socialType2Name SocialType.Google)
                (socialType2Name SocialType.Twitter) ] }

    type ApplicationInfoEnv =
        { DataContext: DbDataContext
          PerimeterSocialProviders: PerimeterSocialProviders }

    let getApplicationInfo (clientId: ClientId) (dataContext: DbDataContext) =
        task {
            if clientId = DEFAULT_CLIENT_ID then
                return getPerimeter ()
            else
                let! (isManagement, result) = getCommon clientId dataContext

                return
                    match isManagement with
                    | true -> getPerimeter ()
                    | false -> result
        }
