namespace PRR.Domain.Tenant

open Common.Domain.Models
open Common.Domain.Utils
open Common.Domain.Utils.CRUD
open PRR.Data.DataContext
open PRR.Data.Entities
open System
open System.Threading.Tasks

module Applications =

    [<CLIMutable>]
    type PostLike =
        { Name: string }

    [<CLIMutable>]
    type GetLike =
        { Id: int
          Name: string
          ClientId: string
          ClientSecret: string
          DateCreated: DateTime
          IdTokenExpiresIn: int
          RefreshTokenExpiresIn: int }

    type CreateEnv =
        { HashProvider: HashProvider
          IdTokenExpiresIn: int<minutes>
          RefreshTokenExpiresIn: int<minutes> }

    let catch =
        function
        | UniqueConstraintException "IX_Applications_DomainId_Name" (ConflictErrorField("name", UNIQUE)) ex ->
            raise ex
        | ex ->
            raise ex

    let create (env: CreateEnv): Create<DomainId * PostLike, int, DbDataContext> =
        createCatch<Application, _, _, _> catch (fun (domainId, dto) ->
            Application
                (Name = dto.Name, ClientId = Guid.NewGuid().ToString(), DomainId = domainId,
                 ClientSecret = env.HashProvider(), IdTokenExpiresIn = int env.IdTokenExpiresIn,
                 RefreshTokenExpiresIn = int env.RefreshTokenExpiresIn, Flow = FlowType.PKCE, AllowedCallbackUrls = "*"))
            (fun x -> x.Id)

    let update: Update<int, DomainId * PostLike, DbDataContext> =
        updateCatch<Application, _, _, _> catch (fun id -> Application(Id = id))
            (fun (_, dto) entity -> entity.Name <- dto.Name)

    let remove: Remove<int, DbDataContext> =
        remove (fun id -> Application(Id = id))

    let getOne: GetOne<int, GetLike, DbDataContext> =
        getOne<Application, _, _, _> (<@ fun p id -> p.Id = id @>)
            (<@ fun p ->
                { Id = p.Id
                  Name = p.Name
                  ClientId = p.ClientId
                  ClientSecret = p.ClientSecret
                  DateCreated = p.DateCreated
                  IdTokenExpiresIn = p.IdTokenExpiresIn
                  RefreshTokenExpiresIn = p.RefreshTokenExpiresIn } @>)

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
            <@ fun (domain: Application) ->
                let like = %(ilike filterValue)
                like domain.Name @>

    let getSortFieldExpr =
        function
        | SortField.DateCreated -> SortDate <@ fun (domain: Application) -> domain.DateCreated @>
        | SortField.Name -> SortString <@ fun (domain: Application) -> domain.Name @>

    let getList: GetList =
        fun dataContext (domainId, prms) ->

            let apps =
                handleListQuery dataContext.Applications getFilterFieldExpr getSortFieldExpr prms

            query {
                for p in apps do
                    where (p.DomainId = domainId && p.IsDomainManagement = false)
                    select
                        { Id = p.Id
                          Name = p.Name
                          ClientId = p.ClientId
                          ClientSecret = p.ClientSecret
                          DateCreated = p.DateCreated
                          IdTokenExpiresIn = p.IdTokenExpiresIn
                          RefreshTokenExpiresIn = p.RefreshTokenExpiresIn }
            }
            |> executeListQuery prms
