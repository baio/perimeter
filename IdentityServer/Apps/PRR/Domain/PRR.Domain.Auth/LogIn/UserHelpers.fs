namespace PRR.Domain.Auth.LogIn

open System
open System.Threading.Tasks
open Common.Domain.Models
open Common.Domain.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open PRR.Domain.Auth
open System.Linq

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

    let getDefaultClientId (dataContext: DbDataContext) email =
        task {

            let! result =
                query {
                    for dur in dataContext.DomainUserRole do
                        where
                            (dur.UserEmail = email
                             && (dur.Role.IsTenantManagement
                                 || dur.Role.IsDomainManagement))
                        select
                            (Tuple.Create
                                (dur.RoleId,
                                 (if dur.Domain.Tenant = null
                                  then dur.Domain.Applications.First(fun x -> x.IsDomainManagement).ClientId
                                  else dur.Domain.Applications.First().ClientId)))
                }
                |> toListAsync

            let priorityRoles =
                [ Seed.Roles.TenantOwner.Id
                  Seed.Roles.DomainOwner.Id
                  Seed.Roles.TenantSuperAdmin.Id
                  Seed.Roles.TenantAdmin.Id
                  Seed.Roles.DomainSuperAdmin.Id
                  Seed.Roles.DomainAdmin.Id ]

            let sortedResult =
                result
                |> Seq.sortBy (fun x -> Seq.findIndex (fun y -> y = fst x) priorityRoles)

            return sortedResult |> Seq.map snd |> Seq.tryHead
        }

    let getClientAppInfo (dataContext: DbDataContext) clientId =
        task {
            let! (issuer, idTokenExpiresIn, isDomainManagement, isTenantManagement) =
                query {
                    for app in dataContext.Applications do
                        where (app.ClientId = clientId)
                        select
                            (app.Domain.Issuer, app.IdTokenExpiresIn, app.IsDomainManagement, app.Domain.Tenant <> null)
                }
                |> LinqHelpers.toSingleExnAsync (unexpected (sprintf "ClientId %s is not found" clientId))

            return { ClientId = clientId
                     Issuer = issuer
                     IdTokenExpiresIn = idTokenExpiresIn * 1<minutes>
                     Type = getAppType (isDomainManagement, isTenantManagement) }
        }

    let getAppInfo (dataContext: DbDataContext) clientId email idTokenExpires =
        task {
            if clientId = DEFAULT_CLIENT_ID then
                printfn "getAppInfo:DEFAULT_CLIENT_ID"
                let! clientId' = getDefaultClientId dataContext email

                return! match clientId' with
                        | Some clientId -> getClientAppInfo dataContext clientId
                        | None ->
                            { ClientId = PERIMETER_CLIENT_ID
                              Issuer = PERIMETER_ISSUER
                              IdTokenExpiresIn = idTokenExpires
                              Type = PerimeterManagement }
                            |> Task.FromResult

            else
                printfn "getAppInfo:2"
                return! getClientAppInfo dataContext clientId
        }
