namespace PRR.Domain.Tenant

open Common.Domain.Models
open Common.Domain.Utils
open Common.Domain.Utils.CRUD
open Common.Utils
open PRR.Data.DataContext
open PRR.Data.Entities
open System
open System.Linq

module Applications =

    [<CLIMutable>]
    type PostLike =
        { Name: string
          ClientId: string }

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

    let create (env: CreateEnv): Create<DomainId * PostLike, int, DbDataContext> =
        create<Application, _, _, _>
            (fun (domainId, dto) ->
            Application
                (Name = dto.Name, ClientId = dto.ClientId, DomainId = domainId, ClientSecret = env.HashProvider(),
                 IdTokenExpiresIn = int env.IdTokenExpiresIn, RefreshTokenExpiresIn = int env.RefreshTokenExpiresIn))
            (fun x -> x.Id)

    let update: Update<int, DomainId * PostLike, DbDataContext> =
        update<Application, _, _, _> (fun id -> Application(Id = id)) (fun (_, dto) entity ->
            entity.Name <- dto.Name
            entity.ClientId <- dto.ClientId)

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
