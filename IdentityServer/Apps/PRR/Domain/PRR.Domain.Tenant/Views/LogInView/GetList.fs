namespace PRR.Domain.Tenant.Views.LogInView

open System.Threading.Tasks
open PRR.Domain.Models
open MongoDB.Bson
open MongoDB.Driver
open FSharp.MongoDB.Driver
open PRR.Domain.Common.Events
open DataAvail.ListQuery.Core
open DataAvail.ListQuery.Mongo

[<AutoOpen>]
module GetList =

    type SortField = DateTime

    type FilterField =
        | Email
        | DateTime
        | AppIdentifier

    type ListQuery = ListQuery<SortField, FilterField>

    [<CLIMutable>]
    type LogIn =
        {
          Id: BsonObjectId
          Social: Social option
          DateTime: System.DateTime
          UserId: int
          ClientId: string
          DomainId: int
          IsManagementClient: bool
          AppIdentifier: string
          UserEmail: string }
    
    [<CLIMutable>]
    type ListResponse = ListQueryResult<LogIn>

    type GetList = IMongoDatabase -> (DomainId * bool * ListQuery) -> Task<ListResponse>

    let getFilterFieldExpr filterValue =
        function
        | FilterField.Email ->
            let likeFilterValue = getILikeString filterValue
            <@ fun (doc: LogIn) -> doc.UserEmail =~ likeFilterValue @>
        | FilterField.DateTime ->
            let date = System.DateTime.Parse(filterValue)
            <@ fun (doc: LogIn) -> doc.DateTime = date @>
        | FilterField.AppIdentifier -> <@ fun (doc: LogIn) -> doc.AppIdentifier = filterValue @>

    let getSortFieldExpr =
        function
        | SortField.DateTime -> SortDate <@ fun (doc: LogIn) -> doc.DateTime @>

    let getLogInList: GetList =
        fun db (domainId, isManagement, prms) ->
            let col =
                db.GetCollection<LogIn>(LOGIN_VIEW_COLLECTION_NAME)

            handleListQuery col getFilterFieldExpr getSortFieldExpr prms
            |> andWhere
                <@ fun doc ->
                    doc.IsManagementClient = isManagement
                    && doc.DomainId = domainId @>
            |> executeListQuery prms
