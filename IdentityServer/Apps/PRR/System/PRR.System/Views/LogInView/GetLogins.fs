namespace PRR.System.Views.LogInView

open System.Threading.Tasks
open Common.Domain.Models
open MongoDB.Driver
open FSharp.Mongo.ListQuery
open FSharp.MongoDB.Driver

[<AutoOpen>]
module GetLogins =

    //
    type SortField = DateTime

    type FilterField =
        | Email
        | DateTime
        | AppIdentifier

    type ListQuery = ListQuery<SortField, FilterField>

    [<CLIMutable>]
    type ListResponse = ListResponse<Doc>

    type GetList = IMongoDatabase -> (DomainId * bool * ListQuery) -> Task<ListResponse>

    let getFilterFieldExpr filterValue =
        function
        | FilterField.Email -> <@ fun (doc: Doc) -> doc.email = filterValue @>
        | FilterField.DateTime ->
            let date = System.DateTime.Parse(filterValue)
            <@ fun (doc: Doc) -> doc.dateTime = date @>
        | FilterField.AppIdentifier -> <@ fun (doc: Doc) -> doc.appIdentifier = filterValue @>

    let getSortFieldExpr =
        function
        | SortField.DateTime -> SortDate <@ fun (doc: Doc) -> doc.dateTime @>

    let getList: GetList =
        fun db (domainId, isManagement, prms) ->

            let col =
                db.GetCollection<Doc>(LOGIN_VIEW_COLLECTION_NAME)

            handleListQuery col getFilterFieldExpr getSortFieldExpr prms
            |> andWhere
                <@ fun doc ->
                    doc.isManagementClient = isManagement
                    && doc.domainId = domainId @>
            |> executeListQuery prms
