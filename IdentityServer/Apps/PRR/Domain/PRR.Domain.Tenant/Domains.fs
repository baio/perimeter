namespace PRR.Domain.Tenant

open Common.Domain.Models
open Common.Domain.Utils
open Common.Domain.Utils.CRUD
open Common.Utils
open PRR.Data.DataContext
open PRR.Data.Entities
open System
open System.Linq

module Domains =

    [<CLIMutable>]
    type PostLike =
        { EnvName: string }

    [<CLIMutable>]
    type ItemGetLike =
        { Id: int
          Name: string }

    [<CLIMutable>]
    type GetLike =
        { Id: int
          EnvName: string
          DateCreated: System.DateTime
          Applications: ItemGetLike seq
          Apis: ItemGetLike seq
          Roles: ItemGetLike seq }

    let checkUserForbidden (userId: UserId) (domainPoolId: DomainPoolId) (domainId: DomainId)
        (dataContext: DbDataContext) =
        query {
            for dp in dataContext.Domains do
                where (dp.Id = domainId && dp.PoolId = Nullable(domainPoolId) && dp.Pool.Tenant.UserId = userId)
                select dp.Id
        }
        |> notFoundRaiseError Forbidden

    let catch =
        function
        | UniqueConstraintException "IX_Domains_PoolId_EnvName" (ConflictErrorField("envName", UNIQUE)) ex ->
            raise ex
        | ex ->
            raise ex

    let create: Create<DomainPoolId * PostLike, int, DbDataContext> =
        createCatch<Domain, _, _, _> catch
            (fun (domainPoolId, dto) -> Domain(PoolId = Nullable(domainPoolId), EnvName = dto.EnvName, IsMain = false))
            (fun x -> x.Id)

    let update: Update<int, PostLike, DbDataContext> =
        updateCatch<Domain, _, _, _> catch (fun id -> Domain(Id = id))
            (fun dto entity -> entity.EnvName <- dto.EnvName)

    let remove: Remove<int, DbDataContext> =
        remove (fun id -> Role(Id = id))

    let getOne: GetOne<int, GetLike, DbDataContext> =
        getOne<Domain, _, _, _> (<@ fun p id -> p.Id = id @>)
            (<@ fun p ->
                { Id = p.Id
                  EnvName = p.EnvName
                  DateCreated = p.DateCreated
                  Applications =
                      p.Applications.Select(fun x ->
                          { Id = x.Id
                            Name = x.Name })
                  Apis =
                      p.Apis.Select(fun x ->
                          { Id = x.Id
                            Name = x.Name })
                  Roles =
                      p.Roles.Select(fun x ->
                          { Id = x.Id
                            Name = x.Name }) } @>)  