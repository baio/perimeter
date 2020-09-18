﻿namespace PRR.Domain.Tenant

open Common.Domain.Models
open Common.Domain.Utils
open Common.Domain.Utils.CRUD
open FSharp.Control.Tasks.V2.ContextInsensitive
open FSharpx
open Newtonsoft.Json
open PRR.Data.DataContext
open PRR.Data.Entities
open System
open System.Linq
open Microsoft.EntityFrameworkCore
open Newtonsoft.Json.Converters


module Domains =

    [<CLIMutable>]
    type PostLike = { EnvName: string }

    [<CLIMutable>]
    type PutLike =
        { EnvName: string
          AccessTokenExpiresIn: int
          SigningAlgorithm: string }


    [<CLIMutable>]
    type ItemGetLike = { Id: int; Name: string }

    [<CLIMutable>]
    type GetLike =
        { Id: int
          EnvName: string
          DateCreated: DateTime
          AccessTokenExpiresIn: int
          [<JsonConverter(typeof<StringEnumConverter>)>]
          SigningAlgorithm: SigningAlgorithmType
          HS256SigningSecret: string
          Issuer: string
          Applications: ItemGetLike seq
          Apis: ItemGetLike seq
          Roles: ItemGetLike seq }

    let checkUserForbidden (userId: UserId)
                           (domainPoolId: DomainPoolId)
                           (domainId: DomainId)
                           (dataContext: DbDataContext)
                           =
        query {
            for dp in dataContext.Domains do
                where
                    (dp.Id = domainId
                     && dp.PoolId = Nullable(domainPoolId)
                     && dp.Pool.Tenant.UserId = userId)
                select dp.Id
        }
        |> notFoundRaiseError Forbidden'

    let catch =
        function
        | UniqueConstraintException "IX_Domains_PoolId_EnvName" (ConflictErrorField ("envName", UNIQUE)) ex -> raise ex
        | ex -> raise ex

    let validatePostData (data: PostLike) =
        [| (validateDomainName "envName" data.EnvName) |]
        |> Array.choose id

    let validatePutData (data: PutLike) =
        [| (validateDomainName "envName" data.EnvName)
           (validateContains [| "SH256"; "RS256" |] "envName" data.SigningAlgorithm)
           (validateIntRange (1, 100000) "accessTokenExpiresIn" data.AccessTokenExpiresIn) |]
        |> Array.choose id

    let create (env: Env) (authStringsProvider: AuthStringsProvider) (domainPoolId, dto: PostLike, userId) =

        let dataContext = env.DataContext

        let add x = x |> add dataContext

        let add' x = x |> add' dataContext

        task {

            // TODO : Select only required fields
            let! pool =
                query {
                    for dp in dataContext.DomainPools do
                        where (dp.Id = domainPoolId)
                        select dp
                }
                |> fun q -> q.Include("Tenant")
                |> toSingleUnchangedAsync dataContext

            let domain =
                Domain
                    (Pool = pool,
                     EnvName = dto.EnvName,
                     IsMain = false,
                     Issuer =
                         sprintf
                             "https://%s.%s.%s.perimeter.com/domain/issuer"
                             dto.EnvName
                             pool.Identifier
                             pool.Tenant.Name,
                     AccessTokenExpiresIn = (int env.AuthConfig.AccessTokenExpiresIn),
                     SigningAlgorithm = SigningAlgorithmType.RS256,
                     RS256Params = authStringsProvider.RS256XMLParams())
                |> add'

            createDomainManagementApp env.AuthStringsProvider env.AuthConfig domain
            |> add

            createDomainManagementApi env.AuthConfig domain
            |> add

            let! userEmail =
                query {
                    for user in dataContext.Users do
                        where (user.Id = userId)
                        select (user.Email)
                }
                |> toSingleExnAsync (unexpected "User email is not found")

            createDomainUserRole userEmail domain Seed.Roles.DomainOwner.Id
            |> add

            try
                do! saveChangesAsync dataContext
                return domain.Id
            with ex -> return catch ex
        }

    // TODO : EnvName could not be updated !
    let update: Update<int, PutLike, DbDataContext> =
        updateCatch<Domain, _, _, _> catch (fun id -> Domain(Id = id)) (fun dto entity ->
            entity.EnvName <- dto.EnvName
            entity.AccessTokenExpiresIn <- dto.AccessTokenExpiresIn
            entity.SigningAlgorithm <- Enum.Parse<SigningAlgorithmType>(dto.SigningAlgorithm))

    let remove: Remove<int, DbDataContext> = remove (fun id -> Role(Id = id))

    let getOne: GetOne<int, GetLike, DbDataContext> =
        getOne<Domain, _, _, _>
            (<@ fun p id -> p.Id = id @>)
            (<@ fun p ->
                    { Id = p.Id
                      EnvName = p.EnvName
                      DateCreated = p.DateCreated
                      AccessTokenExpiresIn = p.AccessTokenExpiresIn
                      SigningAlgorithm = p.SigningAlgorithm
                      HS256SigningSecret = p.HS256SigningSecret
                      Issuer = p.Issuer
                      Applications = p.Applications.Select(fun x -> { Id = x.Id; Name = x.Name })
                      Apis = p.Apis.Select(fun x -> { Id = x.Id; Name = x.Name })
                      Roles = p.Roles.Select(fun x -> { Id = x.Id; Name = x.Name }) } @>)
