namespace PRR.Domain.Tenant

open Common.Domain.Models.Exceptions
open Common.Domain.Utils
open Common.Domain.Utils.CRUD
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Data.DataContext
open PRR.Data.Entities
open System
open System.Linq

module Permissions =

    [<CLIMutable>]
    type PostLike =
        { Name: string
          Description: string }

    [<CLIMutable>]
    type GetLike =
        { Id: int
          Name: string
          Description: string
          DateCreated: System.DateTime }

    let create: Create<int * PostLike, int, DbDataContext> =
        fun (apiId, dto) dbContext ->
            let permission =
                Permission(Name = dto.Name, Description = dto.Description, ApiId = Nullable(apiId)) |> add' dbContext
            task {

                // permission name must be unique in domain context
                let! sameNameCount = query {
                                         for perm in dbContext.Permissions do
                                             where
                                                 (perm.Name = dto.Name
                                                  && perm.Api.Domain.Apis.Any(fun f -> f.Id = apiId))
                                             select perm.Id
                                     }
                                     |> toCountAsync

                if sameNameCount > 0 then raise (Conflict "DOMAIN_PERMISSION_NAME_ALREADY_EXISTS")

                try
                    do! saveChangesAsync dbContext
                with
                // This is duplicate checking the case covered before with sameNameCount
                | UniqueConstraintException "IX_Permissions_Name_ApiId" "DOMAIN_PERMISSION_NAME_UNIQUE" ex ->
                    return raise ex
                | ex ->
                    return raise ex

                return permission.Id
            }


    let update: Update<int, PostLike, DbDataContext> =
        update<Permission, _, _, _> (fun id -> Permission(Id = id)) (fun dto entity ->
            entity.Name <- dto.Name
            entity.Description <- dto.Description)

    let remove: Remove<int, DbDataContext> =
        remove (fun id -> Permission(Id = id))

    let getOne: GetOne<int, GetLike, DbDataContext> =
        getOne<Permission, _, _, _> (<@ fun p id -> p.Id = id @>)
            (<@ fun p ->
                { Id = p.Id
                  Name = p.Name
                  Description = p.Description
                  DateCreated = p.DateCreated } @>)
