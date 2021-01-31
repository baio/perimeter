namespace PRR.API.Routes

open PRR.Data.DataContext
open PRR.Data.Entities
open System.Linq
open DataAvail.EntityFramework.Common

[<AutoOpen>]
module GetAudience =

    let private getOne x =
        CRUD.getOne<_, int, string, DbDataContext> x

    /// Get  identifier of the api's domain's management api
    let fromApiId: CRUD.GetOne<int, string, DbDataContext> =
        getOne<Api>
            (<@ fun x id -> x.Id = id @>)
            (<@ fun p -> p.Domain.Apis.Single(fun x -> x.IsDomainManagement = true).Identifier @>)

    let fromDomainId: CRUD.GetOne<int, string, DbDataContext> =
        getOne<Api> (<@ fun x id -> x.DomainId = id && x.IsDomainManagement = true @>) (<@ fun p -> p.Identifier @>)

    let fromDomainPoolId: CRUD.GetOne<int, string, DbDataContext> =
        getOne<Api>
            (<@ fun x id -> x.Domain.Tenant.DomainPools.Any(fun dp -> dp.Id = id) @>)
            (<@ fun p -> p.Identifier @>)
