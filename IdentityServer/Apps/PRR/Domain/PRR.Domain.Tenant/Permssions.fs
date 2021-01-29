namespace PRR.Domain.Tenant

open Common.Domain.Models
open Common.Domain.Models.Exceptions
open Common.Domain.Utils
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

    let create: Create<int * PostLike, int, DbDataContext> =
        fun (apiId, dto) dbContext ->
            let permission =
                Permission
                    (Name = dto.Name, Description = dto.Description, ApiId = Nullable(apiId), IsDefault = dto.IsDefault)
                |> add' dbContext

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

                try
                    do! saveChangesAsync dbContext
                with
                // This is duplicate checking the case covered before with sameNameCount
                | UniqueConstraintException "IX_Permissions_Name_ApiId" (ConflictErrorField ("name", UNIQUE)) ex ->
                    return raise ex
                | ex -> return raise ex

                return permission.Id
            }


    let update: Update<int, PostLike, DbDataContext> =
        fun data dbContext ->
            update<Permission, _, PostLike, _> (fun id -> Permission(Id = id)) (fun dto entity ->
                entity.Name <- dto.Name
                entity.Description <- dto.Description
                entity.IsDefault <- dto.IsDefault
                dbContext.Entry(entity).Property("IsDefault").IsModified <- true) data dbContext

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

    type FilterField = Text

    type ListQuery = ListQuery<SortField, FilterField>

    [<CLIMutable>]
    type ListResponse = ListQueryResult<GetLike>

    type GetList = DbDataContext -> (int * ListQuery) -> Task<ListResponse>

    let getFilterFieldExpr filterValue =
        function
        | FilterField.Text ->
            <@ fun (domain: Permission) ->
                let like = %(ilike filterValue)
                (like domain.Name) || (like domain.Description) @>

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

    let getAllDomainPermissions (domainId: DomainId) (dataContext: DbDataContext) =
        query {
            for p in dataContext.Permissions do
                where (p.Api.DomainId = domainId)
                select
                    { Id = p.Id
                      Name = p.Name
                      Description = p.Description
                      DateCreated = p.DateCreated
                      IsDefault = p.IsDefault }
        }
        |> toListAsync
