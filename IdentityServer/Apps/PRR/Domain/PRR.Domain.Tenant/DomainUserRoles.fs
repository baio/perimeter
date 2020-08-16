namespace PRR.Domain.Tenant

open Common.Domain.Models
open Common.Domain.Models.Exceptions
open Common.Domain.Utils
open Common.Domain.Utils.CRUD
open Common.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open PRR.Data.Entities
open System
open System.Linq
open System.Threading.Tasks

module DomainUserRoles =

    [<CLIMutable>]
    type PostLike =
        { UserEmail: string
          RolesIds: int seq }

    [<CLIMutable>]
    type RolesGetLike =
        { Id: int
          Name: string }

    [<CLIMutable>]
    type GetLike =
        { UserEmail: string
          Roles: RolesGetLike seq }

    let private validateRoles (domainId: DomainId, dto: PostLike) (dataContext: DbDataContext) =
        query {
            for p in dataContext.Roles do
                where
                    (((p.Domain <> null && p.DomainId <> Nullable(domainId)) || (p.IsTenantManagement = true))
                     && (%in' (dto.RolesIds)) p.Id)
                select p.Id
        }
        |> toCountAsync
        |> map (fun cnt ->
            if cnt > 0 then raise Forbidden)

    let private checkForbidenRoles (forbidenRolesRoles: int seq) (dto: PostLike) =
        dto.RolesIds
        |> Seq.filter (forbidenRolesRoles.Contains)
        |> Seq.length
        |> fun cnt ->
            if cnt > 0 then raise Forbidden

    let updateDomainRoles forbidenRoles ((domainId, dto): DomainId * PostLike) (dbContext: DbDataContext) =
        task {
            checkForbidenRoles forbidenRoles dto
            do! validateRoles (domainId, dto) dbContext
            let incomingDur =
                dto.RolesIds
                |> Seq.map
                    (fun roleId -> DomainUserRole(RoleId = roleId, UserEmail = dto.UserEmail, DomainId = domainId))

            do! (query {
                     for dur in dbContext.DomainUserRole do
                         where (dur.UserEmail = dto.UserEmail && dur.DomainId = domainId)
                         select dur
                 }
                 |> querySplitAddRemoveRange dbContext incomingDur)

            do! saveChangesAsync dbContext
        }

    //

    type SortField = UserEmail

    type FilterField = UserEmail

    type ListQuery = ListQuery<SortField, FilterField>

    [<CLIMutable>]
    type ListResponse = ListResponse<GetLike>

    type GetList = DbDataContext -> (DomainId * ListQuery) -> Task<ListResponse>


    let getFilterFieldExpr filterValue = function
        | FilterField.UserEmail ->
            <@ fun (x: string) ->
                let like = %(ilike filterValue)
                like x @>

    let getSortFieldExpr =
        function
        | SortField.UserEmail -> SortString <@ fun (x: string) -> x @>

    let getList: GetList =
        fun dataContext (domainId, prms) ->

            let dur =
                (query {
                    for p in dataContext.DomainUserRole do
                        select p.UserEmail
                 })
                    .Distinct()

            let dur2 = handleListQuery dur getFilterFieldExpr getSortFieldExpr prms

            query {
                for userEmail in dur2 do
                    join p in dataContext.DomainUserRole on (userEmail = p.UserEmail)
                    where (p.DomainId = domainId)
                    select
                        (Tuple.Create
                            (p.UserEmail,
                             { Id = p.Role.Id
                               Name = p.Role.Name }))
            }
            |> executeGroupByQuery prms (fun (userEmail, roles) ->
                   { UserEmail = userEmail
                     Roles = roles }) dur
