namespace PRR.Domain.Tenant

open Common.Domain.Models
open Common.Domain.Utils
open Common.Domain.Utils.CRUD
open Common.Utils
open PRR.Data.DataContext
open PRR.Data.Entities
open System
open System.Linq

module Apis =

    [<CLIMutable>]
    type PostLike =
        { Name: string
          Identifier: string
          PermissionIds: int seq }

    [<CLIMutable>]
    type PermissionGetLike =
        { Id: int
          Name: string }

    [<CLIMutable>]
    type GetLike =
        { Id: int
          Name: string
          Identifier: string
          DateCreated: System.DateTime
          Permissions: PermissionGetLike seq }

    let private validatePermissions (_, (domainId, dto: PostLike)) (dataContext: DbDataContext) =
        query {
            for p in dataContext.Permissions do
                where (p.Api.DomainId <> domainId && (%in' (dto.PermissionIds)) p.Id)
                select p.Id
        }
        |> toCountAsync
        |> map (fun cnt ->
            if cnt > 0 then raise Forbidden)

    type CreateEnv =
        { AccessTokenExpiresIn: int<minutes> }

    let catch =
        function
        | UniqueConstraintException "IX_Apis_DomainId_Name" (ConflictErrorField("name", UNIQUE)) ex ->
            raise ex
        | ex ->
            raise ex

    let create (env: CreateEnv): Create<DomainId * PostLike, int, DbDataContext> =
        validateCreateCatch<Api, _, _, _> catch (doublet() >> validatePermissions) (fun (domainId, dto) ->
            Api
                (Name = dto.Name, Identifier = dto.Identifier, DomainId = domainId, IsUserManagement = false,
                 AccessTokenExpiresIn = int env.AccessTokenExpiresIn,
                 Permissions =
                     (dto.PermissionIds
                      |> Seq.map (fun id -> Permission(Id = id))
                      |> Seq.toArray))) (fun x -> x.Id)

    let update: Update<int, DomainId * PostLike, DbDataContext> =
        validateUpdateCatch<Api, _, _, _> catch validatePermissions (fun id -> Api(Id = id)) (fun (_, dto) entity ->
            entity.Name <- dto.Name
            entity.Identifier <- dto.Identifier
            entity.Permissions <-
                (dto.PermissionIds
                 |> Seq.map (fun id -> Permission(Id = id))
                 |> Seq.toArray))

    let remove: Remove<int, DbDataContext> =
        remove (fun id -> Api(Id = id))

    let getOne: GetOne<int, GetLike, DbDataContext> =
        getOne<Api, _, _, _> (<@ fun p id -> p.Id = id @>)
            (<@ fun p ->
                { Id = p.Id
                  Name = p.Name
                  Identifier = p.Identifier
                  DateCreated = p.DateCreated
                  Permissions =
                      p.Permissions.Select(fun x ->
                          { Id = x.Id
                            Name = x.Name }) } @>)
