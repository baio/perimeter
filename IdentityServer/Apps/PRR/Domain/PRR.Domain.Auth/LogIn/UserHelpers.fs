namespace PRR.Domain.Auth.LogIn

open Common.Domain.Models
open Common.Domain.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open PRR.Domain.Auth

[<AutoOpen>]
module internal UserHelpers =

    let private DEFAULT_CLIENT_ID = "__DEFAULT_CLIENT_ID__"

    let getUserId (dataContext: DbDataContext) (email, password) =
        query {
            for user in dataContext.Users do
                where (user.Email = email && user.Password = password)
                select user.Id
        }
        |> toSingleOptionAsync


    type AppType =
        | Regular
        | DomainManagement
        | TenantManagement
        | PerimeterManagement

    // TODO : Remove
    type AppInfo =
        { ClientId: ClientId
          Issuer: Issuer
          IdTokenExpiresIn: int<minutes>
          Type: AppType }

    let private getAppType =
        function
        | (true, false) -> DomainManagement
        | (true, true) -> TenantManagement
        | (false, false) -> Regular
        | _ -> raise (unexpected "App both domain and tenant management")

    let getAppInfo (dataContext: DbDataContext) clientId email idTokenExpires =
        task {
            if clientId = DEFAULT_CLIENT_ID then
                printfn "getAppInfo:DEFAULT_CLIENT_ID"
                let! res =
                    query {
                        for app in dataContext.Applications do
                            where (app.Domain.Tenant.User.Email = email)
                            select
                                (app.ClientId,
                                 app.Domain.Issuer,
                                 app.IdTokenExpiresIn,
                                 app.IsDomainManagement,
                                 app.Domain.Tenant <> null)
                    }
                    |> LinqHelpers.toSingleOptionAsync

                return match res with
                       | Some (clientId, issuer, idTokenExpiresIn, isDomainManagement, isTenantManagement) ->
                           { ClientId = clientId
                             Issuer = issuer
                             IdTokenExpiresIn = idTokenExpiresIn * 1<minutes>
                             Type = getAppType (isDomainManagement, isTenantManagement) }
                       | None ->
                           { ClientId = PERIMETER_CLIENT_ID
                             Issuer = PERIMETER_ISSUER
                             IdTokenExpiresIn = idTokenExpires
                             Type = PerimeterManagement }

            else
                printfn "getAppInfo:2"
                let! (issuer, idTokenExpiresIn, isDomainManagement, isTenantManagement) =
                    query {
                        for app in dataContext.Applications do
                            where (app.ClientId = clientId)
                            select
                                (app.Domain.Issuer,
                                 app.IdTokenExpiresIn,
                                 app.IsDomainManagement,
                                 app.Domain.Tenant <> null)
                    }
                    |> LinqHelpers.toSingleExnAsync (unexpected (sprintf "ClientId %s is not found" clientId))

                printfn "getAppInfo:3 %s %i" issuer idTokenExpires

                return { ClientId = clientId
                         Issuer = issuer
                         IdTokenExpiresIn = idTokenExpiresIn * 1<minutes>
                         Type = getAppType (isDomainManagement, isTenantManagement) }
        }
