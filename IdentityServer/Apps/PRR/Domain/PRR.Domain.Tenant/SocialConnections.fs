namespace PRR.Domain.Tenant

open System
open Common.Domain.Utils
open Common.Domain.Models
open Common.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open Helpers
open PRR.Data.DataContext
open PRR.Data.DataContext
open PRR.Data.DataContext.Seed
open PRR.Data.Entities
open PRR.Data.DataContext
open Common.Domain.Utils.CRUD

open PRR.Data.DataContext.Seed
open PRR.Data.Entities

module SocialConnections =

    let catch =
        function
        | UniqueConstraintException "IX_SocialConnections_DomainId_Name" (ConflictErrorField ("name", UNIQUE)) ex ->
            raise ex
        | ex -> raise ex

    type PostLike =
        { ClientId: string
          ClientSecret: string
          Attributes: string []
          Permissions: string [] }

    type GetLike =
        { Name: string
          ClientId: string
          ClientSecret: string
          Attributes: string []
          Permissions: string [] }

    let validateData (data: PostLike) =
        [| (validateNullOrEmpty "clientId" data.ClientId)
           (validateNullOrEmpty "clientSecret" data.ClientSecret) |]
        |> Array.choose id

    let create x =
        x
        |> createCatch catch (fun (domainId, socialName, data: PostLike) ->
               SocialConnection
                   (SocialName = socialName,
                    DomainId = domainId,
                    ClientId = data.ClientId,
                    ClientSecret = data.ClientSecret,
                    Attributes = data.Attributes,
                    Permissions = data.Permissions)) (fun _ -> ())

    let update x =
        x
        |> updateCatch catch (fun (domainId, socialName) ->
               SocialConnection(SocialName = socialName, DomainId = domainId)) (fun (dto: PostLike) entity ->
               entity.ClientId <- dto.ClientId
               entity.ClientSecret <- dto.ClientSecret
               entity.Attributes <- dto.Attributes
               entity.Permissions <- dto.Permissions)

    let delete x =
        x
        |> remove (fun (domainId, socialName) -> SocialConnection(SocialName = socialName, DomainId = domainId))

    let getAll (domainId: DomainId) (dataContext: DbDataContext) =
        query {
            for sc in dataContext.SocialConnections do
                where (sc.DomainId = domainId)
                select sc
        }
        |> toListAsync
        |> TaskUtils.map
            (Seq.map (fun x ->
                { Name = x.SocialName
                  ClientId = x.ClientId
                  ClientSecret = x.ClientSecret
                  Attributes = x.Attributes
                  Permissions = x.Permissions }: GetLike))
