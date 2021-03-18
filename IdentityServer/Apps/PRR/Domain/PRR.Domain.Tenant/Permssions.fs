namespace PRR.Domain.Tenant

open PRR.Domain.Models
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open PRR.Data.Entities
open System
open System.Linq
open System.Threading.Tasks
open DataAvail.EntityFramework.Common
open DataAvail.EntityFramework.Common.CRUD
open DataAvail.ListQuery.Core
open DataAvail.ListQuery.EntityFramework


module Permissions =

    [<CLIMutable>]
    type PostLike =
        { Name: string
          Description: string
          IsDefault: bool }

    [<CLIMutable>]
    type GetLike =
        { Id: int
          Name: string
          Description: string
          DateCreated: DateTime
          IsDefault: bool }

    let create'' (api: Api IdOrEntity, dto: PostLike) =
        let (apiId, api) = IdOrEntity.asPair api
        Permission(Name = dto.Name, Description = dto.Description, Api = api, ApiId = apiId, IsDefault = dto.IsDefault)


    let create': Create<int * PostLike, Permission, DbDataContext> =
        fun (apiId, dtos) dbContext ->
            let permission =
                create'' (Id apiId, dtos) |> add' dbContext

            task {
                try
                    do! saveChangesAsync dbContext
                with
                // This is duplicate checking the case covered before with sameNameCount
                | UniqueConstraintException "IX_Permissions_Name_ApiId" (ConflictErrorField ("name", UNIQUE)) ex ->
                    return raise ex
                | ex -> return raise ex

                return permission
            }

    let create: Create<int * PostLike, int, DbDataContext> =
        fun (apiId, dto) dbContext ->
            task {

                // permission name must be unique in domain context
                let! sameNameCount =
                    query {
                        for perm in dbContext.Permissions do
                            where
                                (perm.Name = dto.Name
                                 && perm.Api.Domain.Apis.Any(fun f -> f.Id = apiId))

                            select perm.Id
                    }
                    |> toCountAsync

                if sameNameCount > 0
                then raise (Conflict(ConflictErrorField("name", UNIQUE)))

                let! result = create' (apiId, dto) dbContext
                return result.Id
            }


    let update: Update<int, PostLike, DbDataContext> =
        fun data dbContext ->
            update<Permission, _, PostLike, _>
                (fun id -> Permission(Id = id))
                (fun dto entity ->
                    entity.Name <- dto.Name
                    entity.Description <- dto.Description
                    entity.IsDefault <- dto.IsDefault
                    dbContext.Entry(entity).Property("IsDefault").IsModified <- true)
                data
                dbContext

    let remove: Remove<int, DbDataContext> = remove (fun id -> Permission(Id = id))

    let getOne: GetOne<int, GetLike, DbDataContext> =
        getOne<Permission, _, _, _>
            (<@ fun p id -> p.Id = id @>)
            (<@ fun p ->
                    { Id = p.Id
                      Name = p.Name
                      Description = p.Description
                      DateCreated = p.DateCreated
                      IsDefault = p.IsDefault } @>)

    //
    type SortField =
        | Name
        | DateCreated

    type FilterField =
        | Text
        | Type

    type ListQuery = ListQuery<SortField, FilterField>

    [<CLIMutable>]
    type ListResponse = ListQueryResult<GetLike>

    type GetList = DbDataContext -> (int * ListQuery) -> Task<ListResponse>

    let getFilterFieldExpr filterValue =
        function
        | FilterField.Text ->
            <@ fun (perm: Permission) ->
                let like = %(ilike filterValue)
                (like perm.Name) || (like perm.Description) @>
        | FilterField.Type ->
            match filterValue with
            | "tenant" -> <@ fun (perm: Permission) -> perm.IsTenantManagement @>
            | "domain" -> <@ fun (perm: Permission) -> perm.IsDomainManagement @>
            | "common" ->
                <@ fun (perm: Permission) ->
                    not perm.IsTenantManagement
                    && not perm.IsDomainManagement @>

    let getSortFieldExpr =
        function
        | SortField.DateCreated -> SortDate <@ fun (perm: Permission) -> perm.DateCreated @>
        | SortField.Name -> SortString <@ fun (perm: Permission) -> perm.Name @>

    let getList: GetList =
        fun dataContext (apiId, prms) ->

            let apps =
                handleListQuery dataContext.Permissions getFilterFieldExpr getSortFieldExpr prms

            query {
                for p in apps do
                    where (p.ApiId = Nullable(apiId))

                    select
                        { Id = p.Id
                          Name = p.Name
                          Description = p.Description
                          DateCreated = p.DateCreated
                          IsDefault = p.IsDefault }
            }
            |> executeListQuery prms

    let getTypedDomainPermissions (domainId: DomainId) (permType: string) (dataContext: DbDataContext) =

        let typeFilter =
            match permType with
            | "common" ->
                <@ fun (p: Permission) ->
                    not p.IsTenantManagement
                    && not p.IsDomainManagement @>
            | "tenant" -> <@ fun (p: Permission) -> p.IsTenantManagement @>
            | "domain" -> <@ fun (p: Permission) -> p.IsDomainManagement @>
            | _ -> <@ fun (_: Permission) -> true @>

        query {
            for p in dataContext.Permissions do
                where (p.Api.DomainId = domainId && (%typeFilter) p)

                select
                    { Id = p.Id
                      Name = p.Name
                      Description = p.Description
                      DateCreated = p.DateCreated
                      IsDefault = p.IsDefault }
        }
        |> toListAsync
