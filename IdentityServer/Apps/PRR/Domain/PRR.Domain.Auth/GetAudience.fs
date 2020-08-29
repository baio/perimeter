namespace PRR.Domain.Auth

open Common.Domain.Utils
open PRR.Data.DataContext
open PRR.Data.Entities
open System.Linq

module GetAudience =

    let private getOne x = CRUD.getOne<_, int, string, DbDataContext> x

    let fromApiId: CRUD.GetOne<int, string, DbDataContext> =
        getOne<Api> (<@ fun x id -> x.Id = id @>) (<@ fun p -> p.Identifier @>)

    let fromDomainId: CRUD.GetOne<int, string, DbDataContext> =
        getOne<Api> (<@ fun x id -> x.DomainId = id && x.IsDomainManagement = true @>) (<@ fun p -> p.Identifier @>)

    let fromDomainPoolId: CRUD.GetOne<int, string, DbDataContext> =
        getOne<Api> (<@ fun x id -> x.Domain.Tenant.DomainPools.Any(fun dp -> dp.Id = id) @>)
            (<@ fun p -> p.Identifier @>)
