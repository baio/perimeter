namespace PRR.Domain.Tenant

open Common.Domain.Models
open Common.Domain.Utils
open Common.Domain.Utils.CRUD
open Common.Utils
open PRR.Data.DataContext
open PRR.Data.Entities
open System
open System.Linq
open System.Threading.Tasks
open FSharp.Control.Tasks.V2.ContextInsensitive

module Apis =

    [<CLIMutable>]
    type PostLike =
        { Name: string
          Identifier: string
          AccessTokenExpiresIn: int }

    [<CLIMutable>]
    type PermissionGetLike = { Id: int; Name: string }

    [<CLIMutable>]
    type GetLike =
        { Id: int
          Name: string
          Identifier: string
          DateCreated: System.DateTime
          AccessTokenExpiresIn: int
          Permissions: PermissionGetLike seq }

    let validateData (data: PostLike) =
        [| (validateNullOrEmpty "name" data.Name)
           (validateDomainName "identifier" data.Identifier) |]
        |> Array.choose id


    type CreateEnv = { AccessTokenExpiresIn: int<minutes> }

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
                     AccessTokenExpiresIn = int env.AccessTokenExpiresIn,
                     Permissions = [||])
                |> add' dataContext

            try
                do! saveChangesAsync dataContext
                return api.Id
            with ex -> return catch ex
        }



    let update: Update<int, DomainId * PostLike, DbDataContext> =
        updateCatch<Api, _, _, _> catch (fun id -> Api(Id = id)) (fun (_, dto) entity -> entity.Name <- dto.Name)

    let remove: Remove<int, DbDataContext> = remove (fun id -> Api(Id = id))

    let getOne: GetOne<int, GetLike, DbDataContext> =
        getOne<Api, _, _, _>
            (<@ fun p id -> p.Id = id @>)
            (<@ fun p ->
                    { Id = p.Id
                      Name = p.Name
                      Identifier = p.Identifier
                      DateCreated = p.DateCreated
                      AccessTokenExpiresIn = p.AccessTokenExpiresIn
                      Permissions = p.Permissions.Select(fun x -> { Id = x.Id; Name = x.Name }) } @>)
    //
    type SortField =
        | Name
        | DateCreated

    type FilterField = Name

    type ListQuery = ListQuery<SortField, FilterField>

    [<CLIMutable>]
    type ListResponse = ListResponse<GetLike>

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
                    select
                        { Id = p.Id
                          Name = p.Name
                          Identifier = p.Identifier
                          DateCreated = p.DateCreated
                          AccessTokenExpiresIn = p.AccessTokenExpiresIn
                          Permissions = p.Permissions.Select(fun x -> { Id = x.Id; Name = x.Name }) }
            }
            |> executeListQuery prms
