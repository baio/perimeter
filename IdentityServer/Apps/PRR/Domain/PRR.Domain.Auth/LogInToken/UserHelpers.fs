namespace PRR.Domain.Auth.LogInToken

open Common.Domain.Models
open Common.Domain.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FSharpx
open Microsoft.FSharp.Linq
open Models
open PRR.Data.DataContext
open PRR.Data.Entities
open System
open System.Linq

[<AutoOpen>]
module internal UserHelpers =

    [<CLIMutable>]
    type TokenData =
        { Id: int
          FirstName: string
          LastName: string
          Email: string }


    let getUserDataForToken' (dataContext: DbDataContext) filterUser =
        query {
            for user in dataContext.Users do
                where ((%filterUser) user)
                select
                    { Id = user.Id
                      FirstName = user.FirstName
                      LastName = user.LastName
                      Email = user.Email }
        }
        |> toSingleOptionAsync


    let getUserDataForToken (dataContext: DbDataContext) userId =
        getUserDataForToken' dataContext <@ fun (user: User) -> (user.Id = userId) @>

    type RolePermissions =
        { Role: string
          Permissions: string seq }

    let getUserDomainRolesPermissions (dataContext: DbDataContext) (domainId, userEmail) =
        query {
            for dur in dataContext.DomainUserRole do
                where (dur.DomainId = domainId && dur.UserEmail = userEmail)
                select
                    { Role = dur.Role.Name
                      Permissions = dur.Role.RolesPermissions.Select(fun x -> x.Permission.Name) }
        }
        |> toListAsync

    type DomainAudiences =
        { DomainId: int
          Audiences: string seq }

    let getClientDomainAudiences (dataContext: DbDataContext) clientId =
        query {
            for app in dataContext.Applications do
                where (app.ClientId = clientId)
                select
                    { DomainId = app.Domain.Id
                      Audiences = app.Domain.Apis.Select(fun x -> x.Identifier) }
        }
        |> toSingleOptionAsync
