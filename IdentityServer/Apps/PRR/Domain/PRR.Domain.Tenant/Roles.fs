namespace PRR.Domain.Tenant

open Common.Domain.Models
open Common.Domain.Utils
open Common.Domain.Utils.CRUD
open Common.Utils
open PRR.Data.DataContext
open PRR.Data.Entities
open System
open System.Linq

module Roles =

    [<CLIMutable>]
    type PostLike =
        { Name: string
          Description: string
          PermissionIds: int seq }

    [<CLIMutable>]
    type PermissionGetLike =
        { Id: int
          Name: string }

    [<CLIMutable>]
    type GetLike =
        { Id: int
          Name: string
          Description: string
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

    let create: Create<DomainId * PostLike, int, DbDataContext> =
        validateCreateCatch<Role, _, _, _> (function
            | UniqueConstraintException "IX_Roles_Name_DomainId" (ConflictErrorField ("name", UNIQUE)) ex ->
                raise ex
            | ex ->
                raise ex) (doublet() >> validatePermissions) (fun (domainId, dto) ->
            let role = Role(Name = dto.Name, Description = dto.Description, DomainId = Nullable(domainId))
            role.RolesPermissions <-
                dto.PermissionIds
                |> Seq.map (fun p -> RolePermission(PermissionId = p))
                |> Seq.toArray
            role) (fun x -> x.Id)

    let update: Update<int, DomainId * PostLike, DbDataContext> =
        validateUpdateTask<Role, _, _, _> validatePermissions (fun id -> Role(Id = id)) (fun dbContext (_, dto) entity ->
            entity.Name <- dto.Name
            entity.Description <- dto.Description
            let incomingRps =
                dto.PermissionIds
                |> Seq.map (fun permissionId -> RolePermission(PermissionId = permissionId, RoleId = entity.Id))
            query {
                for rp in dbContext.RolesPermissions do
                    where (rp.RoleId = entity.Id)
                    select rp
            }
            |> querySplitAddRemoveRange dbContext incomingRps)

    let remove: Remove<int, DbDataContext> =
        remove (fun id -> Role(Id = id))

    let getOne: GetOne<int, GetLike, DbDataContext> =
        getOne<Role, _, _, _> (<@ fun p id -> p.Id = id @>)
            (<@ fun p ->
                { Id = p.Id
                  Name = p.Name
                  Description = p.Description
                  DateCreated = p.DateCreated
                  Permissions =
                      p.RolesPermissions.Select(fun x ->
                          { Id = x.PermissionId
                            Name = x.Permission.Name }) } @>)
