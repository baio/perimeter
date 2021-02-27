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
    type ListResponse = ListQueryResult<LogInDoc>

    type GetList = IMongoDatabase -> (DomainId * bool * ListQuery) -> Task<ListResponse>

    let getFilterFieldExpr filterValue =
        function
        | FilterField.Email ->
            let likeFilterValue = getILikeString filterValue
            <@ fun (doc: LogInDoc) -> doc.UserEmail =~ likeFilterValue @>
        | FilterField.DateTime ->
            let date = System.DateTime.Parse(filterValue)
            <@ fun (doc: LogInDoc) -> doc.DateTime = date @>
        | FilterField.AppIdentifier -> <@ fun (doc: LogInDoc) -> doc.AppIdentifier = filterValue @>

    let getSortFieldExpr =
        function
        | SortField.DateTime -> SortDate <@ fun (doc: LogInDoc) -> doc.DateTime @>

    let getLogInList: GetList =
        fun db (domainId, isManagement, prms) ->
            let col =
                db.GetCollection<LogInDoc>(LOGIN_VIEW_COLLECTION_NAME)

            handleListQuery col getFilterFieldExpr getSortFieldExpr prms
            |> andWhere
                <@ fun doc ->
                    doc.IsManagementClient = isManagement
                    && doc.DomainId = domainId @>
            |> executeListQuery prms
