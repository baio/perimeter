namespace PRR.Domain.Tenant

open Common.Domain.Models
open DataAvail.Common
open PRR.Data.DataContext
open PRR.Data.DataContext.Seed
open PRR.Data.Entities
open System
open System.Linq
open System.Threading.Tasks
open DataAvail.ListQuery.Core
open DataAvail.ListQuery.EntityFramework
open DataAvail.EntityFramework.Common
open DataAvail.EntityFramework.Common.CRUD
open DataAvail.Http.Exceptions

module Roles =

    [<CLIMutable>]
    type GetIdName =
        { Id: int
          Name: string }

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
        |> TaskUtils.map (fun cnt ->
            if cnt > 0 then raise Forbidden')

    let create: Create<DomainId * PostLike, int, DbDataContext> =
        validateCreateCatch<Role, _, _, _> (function
            | UniqueConstraintException "IX_Roles_Name_DomainId" (ConflictErrorField("name", UNIQUE)) ex ->
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
    //
    type SortField =
        | Name
        | DateCreated

    type FilterField = Text

    type ListQuery = ListQuery<SortField, FilterField>

    [<CLIMutable>]
    type ListResponse = ListQueryResult<GetLike>

    type GetList = DbDataContext -> (DomainId * ListQuery) -> Task<ListResponse>

    let getFilterFieldExpr filterValue =
        function
        | FilterField.Text ->
            <@ fun (domain: Role) ->
                let like = %(ilike filterValue)
                (like domain.Name) || (like domain.Description) @>

    let getSortFieldExpr =
        function
        | SortField.DateCreated -> SortDate <@ fun (perm: Role) -> perm.DateCreated @>
        | SortField.Name -> SortString <@ fun (perm: Role) -> perm.Name @>

    let getList: GetList =
        fun dataContext (domainId, prms) ->

            let roles =
                handleListQuery dataContext.Roles getFilterFieldExpr getSortFieldExpr prms

            query {
                for p in roles do
                    where (p.DomainId = Nullable(domainId))
                    select
                        { Id = p.Id
                          Name = p.Name
                          Description = p.Description
                          DateCreated = p.DateCreated
                          Permissions =
                              p.RolesPermissions.Select(fun x ->
                                  { Id = x.PermissionId
                                    Name = x.Permission.Name }) }
            }
            |> executeListQuery prms

    type RoleType =
        | TenantManagement
        | DomainManagement
        | User of DomainId

    let getAllRoles (roleType: RoleType) (dataContext: DbDataContext) =

        let roleTypeFilter =
            match roleType with
            | TenantManagement -> <@ fun (x: Role) -> x.IsTenantManagement && x.Id <> Roles.TenantOwner.Id @>
            | DomainManagement -> <@ fun (x: Role) -> x.IsDomainManagement && x.Id <> Roles.DomainOwner.Id @>
            | User domainId ->
                <@ fun (x: Role) ->
                    x.DomainId = Nullable(domainId) && not x.IsDomainManagement && not x.IsTenantManagement @>

        query {
            for p in dataContext.Roles do
                where ((%roleTypeFilter) p)
                select
                    { Id = p.Id
                      Name = p.Name }
        }
        |> toListAsync
