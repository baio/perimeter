namespace PRR.Domain.Tenant

open Common.Domain.Models
open PRR.Data.DataContext
open PRR.Data.Entities
open System
open System.Linq
open System.Threading.Tasks
open FSharp.Control.Tasks.V2.ContextInsensitive
open DataAvail.ListQuery.Core
open DataAvail.ListQuery.EntityFramework
open DataAvail.EntityFramework.Common
open DataAvail.EntityFramework.Common.CRUD
open DataAvail.Http.Exceptions

module Apis =

    [<CLIMutable>]
    type PostLike = { Name: string; Identifier: string }

    [<CLIMutable>]
    type PutLike = { Name: string }

    [<CLIMutable>]
    type PermissionGetLike =
        { Id: int
          Name: string
          IsDefault: bool }

    [<CLIMutable>]
    type GetLike =
        { Id: int
          Name: string
          IdentifierUri: string
          DateCreated: DateTime
          Permissions: PermissionGetLike seq }

    let validatePostData (data: PostLike) =
        [| (validateNullOrEmpty "name" data.Name)
           (validateDomainName "identifier" data.Identifier) |]
        |> Array.choose id


    let validatePutData (data: PutLike) =
        [| (validateNullOrEmpty "name" data.Name) |]
        |> Array.choose id

    type CreateEnv =
        { AccessTokenExpiresIn: int<minutes>
          HS256SigningSecret: unit -> string }

    let catch =
        function
        | UniqueConstraintException "IX_Apis_DomainId_Name" (ConflictErrorField ("name", UNIQUE)) ex -> raise ex
        | ex -> raise ex

    let create (env: CreateEnv) (domainId, dto: PostLike) (dataContext: DbDataContext) =

        task {
            let! data =
                query {
                    for domain in dataContext.Domains do
                        where (domain.Id = domainId)
                        select (domain.EnvName, domain.Pool.Identifier, domain.Pool.Tenant.Name)
                }
                |> toSingleAsync


            let (envName, poolIdentifier, tenantName) = data

            let api =
                Api
                    (Name = dto.Name,
                     Identifier = (sprintf "https://%s.%s.%s.%s.com" dto.Identifier envName poolIdentifier tenantName),
                     DomainId = domainId,
                     IsDomainManagement = false,
                     Permissions = [||])
                |> add' dataContext

            try
                do! saveChangesAsync dataContext
                return api.Id
            with ex -> return catch ex
        }



    let update: Update<int, DomainId * PutLike, DbDataContext> =
        updateCatch<Api, _, _, _> catch (fun id -> Api(Id = id)) (fun (_, dto) entity -> entity.Name <- dto.Name)

    let remove: Remove<int, DbDataContext> = remove (fun id -> Api(Id = id))

    let private select' =
        <@ fun (p: Api) ->
            { Id = p.Id
              Name = p.Name
              IdentifierUri = p.Identifier
              DateCreated = p.DateCreated
              Permissions =
                  p.Permissions.Select(fun x ->
                      { Id = x.Id
                        Name = x.Name
                        IsDefault = x.IsDefault }) } @>


    let getOne: GetOne<int, GetLike, DbDataContext> =
        getOne<Api, _, _, _> (<@ fun p id -> p.Id = id @>) select'
    //
    type SortField =
        | Name
        | DateCreated

    type FilterField = Name

    type ListQuery = ListQuery<SortField, FilterField>

    [<CLIMutable>]
    type ListResponse = ListQueryResult<GetLike>

    type GetList = DbDataContext -> (TenantId * ListQuery) -> Task<ListResponse>

    let getFilterFieldExpr filterValue =
        function
        | FilterField.Name ->
            <@ fun (domain: Api) ->
                let like = %(ilike filterValue)
                like domain.Name @>

    let getSortFieldExpr =
        function
        | SortField.DateCreated -> SortDate <@ fun (domain: Api) -> domain.DateCreated @>
        | SortField.Name -> SortString <@ fun (domain: Api) -> domain.Name @>

    let getList: GetList =
        fun dataContext (domainId, prms) ->

            let apps =
                handleListQuery dataContext.Apis getFilterFieldExpr getSortFieldExpr prms

            query {
                for p in apps do
                    where
                        (p.DomainId = domainId
                         && p.IsDomainManagement = false)
                    select ((%select') p)
            }
            |> executeListQuery prms
