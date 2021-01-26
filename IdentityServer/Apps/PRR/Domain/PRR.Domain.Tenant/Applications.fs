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
    type PostLike = { Name: string }

    type PutLike =
        { Name: string
          IdTokenExpiresIn: int
          RefreshTokenExpiresIn: int
          AllowedCallbackUrls: string
          AllowedLogoutCallbackUrls: string
          SSOEnabled: bool }

    [<CLIMutable>]
    type GetLike =
        { Id: int
          Name: string
          ClientId: string
          ClientSecret: string
          DateCreated: DateTime
          IdTokenExpiresIn: int
          RefreshTokenExpiresIn: int
          AllowedCallbackUrls: string
          AllowedLogoutCallbackUrls: string
          SSOEnabled: bool
          Flow: FlowType }

    type CreateEnv =
        { AuthStringsProvider: AuthStringsGetter
          IdTokenExpiresIn: int<minutes>
          RefreshTokenExpiresIn: int<minutes> }

    let validatePost (data: PostLike) =
        [| (validateNullOrEmpty "name" data.Name) |]
        |> Array.choose id

    let validatePut (data: PutLike) =
        [| (validateNullOrEmpty "name" data.Name)
           (validateIntRange (1, 9999999) "idTokenExpiresIn" data.IdTokenExpiresIn)
           (validateIntRange (1, 9999999) "refreshTokenExpiresIn" data.RefreshTokenExpiresIn)
           (validateUrlsList true "allowedCallbackUrls" data.AllowedCallbackUrls)
           (validateUrlsList true "allowedLogoutCallbackUrls" data.AllowedLogoutCallbackUrls) |]
        |> Array.choose id

    let catch =
        function
        | UniqueConstraintException "IX_Applications_DomainId_Name" (ConflictErrorField ("name", UNIQUE)) ex -> raise ex
        | ex -> raise ex

    let create (env: CreateEnv): Create<DomainId * PostLike, int, DbDataContext> =
        createCatch<Application, _, _, _> catch (fun (domainId, dto) ->
            Application
                (Name = dto.Name,
                 ClientId = env.AuthStringsProvider.ClientId(),
                 DomainId = domainId,
                 ClientSecret = env.AuthStringsProvider.ClientSecret(),
                 IdTokenExpiresIn = int env.IdTokenExpiresIn,
                 RefreshTokenExpiresIn = int env.RefreshTokenExpiresIn,
                 Flow = FlowType.PKCE,
                 AllowedLogoutCallbackUrls = "*",
                 AllowedCallbackUrls = "*")) (fun x -> x.Id)

    let update: Update<int, DomainId * PutLike, DbDataContext> =
        updateCatch<Application, _, _, _> catch (fun id -> Application(Id = id)) (fun (_, dto) entity ->
            entity.Name <- dto.Name
            entity.IdTokenExpiresIn <- dto.IdTokenExpiresIn
            entity.RefreshTokenExpiresIn <- dto.RefreshTokenExpiresIn
            entity.AllowedCallbackUrls <- dto.AllowedCallbackUrls
            entity.AllowedLogoutCallbackUrls <- dto.AllowedLogoutCallbackUrls
            entity.SSOEnabled <- dto.SSOEnabled)

    let remove: Remove<int, DbDataContext> = remove (fun id -> Application(Id = id))

    let selectApp =
        <@ fun (p: Application) ->
            { Id = p.Id
              Name = p.Name
              ClientId = p.ClientId
              ClientSecret = p.ClientSecret
              DateCreated = p.DateCreated
              IdTokenExpiresIn = p.IdTokenExpiresIn
              RefreshTokenExpiresIn = p.RefreshTokenExpiresIn
              AllowedCallbackUrls = p.AllowedCallbackUrls
              AllowedLogoutCallbackUrls = p.AllowedLogoutCallbackUrls
              SSOEnabled = p.SSOEnabled
              Flow = p.Flow } @>

    let getOne: GetOne<int, GetLike, DbDataContext> =
        getOne<Application, _, _, _> (<@ fun p id -> p.Id = id @>) (selectApp)

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
                    where
                        (p.DomainId = domainId
                         && p.IsDomainManagement = false)
                    select ((%selectApp) p)
            }
            |> executeListQuery prms
