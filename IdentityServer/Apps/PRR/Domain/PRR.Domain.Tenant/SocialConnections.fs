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
            printfn "3333"
            raise ex
        | ex -> raise ex

    type PostLike =
        { ClientId: string
          ClientSecret: string
          Attributes: string []
          Permissions: string [] }

    [<CLIMutable>]
    type GetLike =
        { Name: string
          ClientId: string
          ClientSecret: string
          Attributes: string []
          Permissions: string []
          Order: int }

    let validateData (data: PostLike) =
        [| (validateNullOrEmpty "clientId" data.ClientId)
           (validateNullOrEmpty "clientSecret" data.ClientSecret) |]
        |> Array.choose id

    let private getItem =
        <@ fun (x: SocialConnection) ->
            { Name = x.SocialName
              ClientId = x.ClientId
              ClientSecret = x.ClientSecret
              Attributes = x.Attributes
              Permissions = x.Permissions
              Order = x.Order }: GetLike @>


    let create dto (dataContext: DbDataContext) =
        task {
            let (domainId, _, _) = dto

            let! orders =
                query {
                    for sc in dataContext.SocialConnections do
                        where (sc.DomainId = domainId)
                        select sc.Order
                }
                |> toListAsync

            let maxOrder = orders |> Seq.append [ -1 ] |> Seq.max

            return! createCatch catch (fun (domainId, socialName, data: PostLike) ->
                        SocialConnection
                            (SocialName = socialName,
                             DomainId = domainId,
                             ClientId = data.ClientId,
                             ClientSecret = data.ClientSecret,
                             Attributes = data.Attributes,
                             Permissions = data.Permissions,
                             Order = maxOrder + 1)) (fun _ -> ()) dto dataContext
        }

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
                select ((%getItem) sc)
        }
        |> toListAsync

    let reorder (domainId: DomainId) (data: Map<string, int>) (dataContext: DbDataContext) =
        data
        |> Map.toSeq
        |> Seq.map (fun (k, _) -> SocialConnection(SocialName = k, DomainId = domainId))
        |> updateRange dataContext (fun sc ->
               sc.Order <- data.Item(sc.SocialName)
               dataContext.Entry(sc).Property("Order").IsModified <- true)
        saveChangesAsync dataContext

    let getAllByClientId (clientId: ClientId) (dataContext: DbDataContext) =
        query {
            for sc in dataContext.SocialConnections do
                join app in dataContext.Applications on (sc.DomainId = app.DomainId)
                where (app.ClientId = clientId)
                select ((%getItem) sc)
        }
        |> toListAsync
