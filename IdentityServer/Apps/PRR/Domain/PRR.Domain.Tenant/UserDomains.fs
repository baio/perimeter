namespace PRR.Domain.Tenant

open PRR.Domain.Models
open DomainUserRoles
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open System
open System.Linq
open System.Threading.Tasks
open DataAvail.EntityFramework.Common
open DataAvail.Http.Exceptions

module UserDomains =

    [<CLIMutable>]
    type ItemGetLike = { Id: int; Name: string }

    [<CLIMutable>]
    type TenantDomain =
        { Id: int
          Tenant: ItemGetLike
          Pool: ItemGetLike
          EnvName: string
          ManagementClientId: string
          IsTenantManagement: bool
          Roles: ItemGetLike array }

    [<CLIMutable>]
    type TenantDomainSingle =
        { Id: int
          Tenant: ItemGetLike
          Pool: ItemGetLike
          EnvName: string
          ManagementClientId: string
          IsTenantManagement: bool
          Role: ItemGetLike }

    let getClientDomains (dataContext: DbDataContext) (userId: UserId) =
        task {

            let! userEmail =
                query {
                    for user in dataContext.Users do
                        where (user.Id = userId)
                        select user.Email
                }
                |> toSingleExnAsync (unAuthorized "User is not found")

            let! items =
                query {
                    for p in dataContext.DomainUserRole do
                        where
                            (p.UserEmail = userEmail
                             && (p.Role.IsDomainManagement
                                 || p.Role.IsTenantManagement))

                        select
                            { Id = p.Domain.Id
                              Tenant =
                                  if p.Domain.Tenant <> null then
                                      { Id = p.Domain.Tenant.Id
                                        Name = p.Domain.Tenant.Name }
                                  else
                                      { Id = p.Domain.Pool.Tenant.Id
                                        Name = p.Domain.Pool.Tenant.Name }
                              Pool =
                                  { Id = p.Domain.Pool.Id
                                    Name = p.Domain.Pool.Name }
                              EnvName = p.Domain.EnvName
                              ManagementClientId =
                                  (p.Domain.Applications.FirstOrDefault(fun p -> p.IsDomainManagement = true))
                                      .ClientId
                              IsTenantManagement = p.Domain.Pool = null
                              Role = { Id = p.Role.Id; Name = p.Role.Name } }
                }
                |> toListAsync

            let res =
                items
                |> Seq.groupBy (fun x -> x.Id)
                |> Seq.map (fun (id, items) ->
                    let head = items |> Seq.head
                    let roles = items |> Seq.map (fun x -> x.Role)

                    { Id = head.Id
                      Tenant = head.Tenant
                      Pool = head.Pool
                      EnvName = head.EnvName
                      ManagementClientId = head.ManagementClientId
                      IsTenantManagement = head.IsTenantManagement
                      Roles = roles |> Seq.toArray })

            return res
        }
