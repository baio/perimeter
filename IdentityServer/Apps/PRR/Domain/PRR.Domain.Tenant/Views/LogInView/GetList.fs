namespace PRR.Domain.Tenant.Views.LogInView

open Common.Domain.Models
open MongoDB.Driver
open FSharp.Mongo.ListQuery
open FSharp.MongoDB.Driver
open MongoDB.Driver
open PRR.Domain.Common.Events

module GetList = 

    type SortField = DateTime

    type FilterField =
        | Email
        | DateTime
        | AppIdentifier

    type ListQuery = ListQuery<SortField, FilterField>

    [<CLIMutable>]
    type ListResponse = ListResponse<LogIn>

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
        | SortField.DateTime -> SortDate <@ fun (doc: Doc) -> doc.dateTime @>

    let getList: GetList =
        fun db (domainId, isManagement, prms) ->
            let col =
                db.GetCollection<LogIn>(LOGIN_VIEW_COLLECTION_NAME)
            handleListQuery col getFilterFieldExpr getSortFieldExpr prms
            |> andWhere
                <@ fun doc ->
                    doc.IsManagementClient = isManagement
                    && doc.DomainId = domainId @>
            |> executeListQuery prms
