namespace PRR.Domain.Auth.LogInToken

open Common.Domain.Models
open Common.Domain.Utils
open Common.Utils
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
          Email: string
          SocialType: SocialType option }


    let private getUserDataForTokenNotSocial (dataContext: DbDataContext) filterUser =
        query {
            for user in dataContext.Users do
                where ((%filterUser) user)
                select
                    { Id = user.Id
                      FirstName = user.FirstName
                      LastName = user.LastName
                      Email = user.Email
                      SocialType = None }
        }
        |> toSingleOptionAsync

    let private getUserDataForTokenSocial (dataContext: DbDataContext) filterUser socialType =
        query {
            for si in dataContext.SocialIdentities do
                where ((%filterUser) si.User)
                select (si.Name, si.Email, si.UserId)
        }
        |> toSingleOptionAsync
        |> TaskUtils.map
            (Option.map (fun (name, email, userId) ->
                let (firstName, lastName) = splitName name
                { Id = userId
                  FirstName = firstName
                  LastName = lastName
                  Email = email
                  SocialType = Some socialType }: TokenData))

    let getUserDataForToken' (dataContext: DbDataContext) filterUser socialType =
        match socialType with
        | Some socialType -> getUserDataForTokenSocial dataContext filterUser socialType
        | None -> getUserDataForTokenNotSocial dataContext filterUser

    let getUserDataForToken (dataContext: DbDataContext) userId =
        getUserDataForToken' dataContext <@ fun (user: User) -> (user.Id = userId) @>

    type RolePermissions =
        { Role: string
          Permissions: string seq }

    let getUserDomainRolesPermissions (dataContext: DbDataContext) (domainId, userEmail) =
        query {
            for dur in dataContext.DomainUserRole do
                where
                    (dur.DomainId = domainId
                     && dur.UserEmail = userEmail)
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
