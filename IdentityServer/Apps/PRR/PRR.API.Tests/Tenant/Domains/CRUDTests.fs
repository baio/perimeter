namespace PRR.API.Tests.Tenant.Domains

open Akkling
open Common.Test.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FsUnit
open PRR.API.Tests.Utils
open PRR.Domain.Auth.SignUp
open PRR.Domain.Tenant.Domains
open PRR.System.Models
open Xunit
open Xunit.Abstractions
open Xunit.Priority

module CRUD =
    let userData: Data =
        { FirstName = "First"
          LastName = "Last"
          Email = "user@user.com" }

    let userPassword = "123"

    let mutable userToken: string = null

    let mutable testContext: UserTestContext option = None

    let mutable domainId: int option = None


    [<CLIMutable>]
    type GetLikeItemDto =
        { id: int
          name: string
          dateCreated: System.DateTime }

    [<CLIMutable>]
    type GetLikeDto =
        { id: int
          envName: string
          dateCreated: System.DateTime
          roles: GetLikeItemDto seq
          applications: GetLikeItemDto seq
          apis: GetLikeItemDto seq }

    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``domain-crud-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.``0 BEFORE ALL``() =
            task {
                testContext <- Some(createUserTestContext testFixture)
                let! userToken' = createUser' false testContext.Value userData userPassword
                userToken <- userToken'
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``A Create domain must be success``() =
            let domainPoolId = testContext.Value.GetTenant().DomainPoolId
            task {
                let data: PostLike =
                    { EnvName = "Stage" }
                let! result = testFixture.HttpPostAsync userToken
                                  (sprintf "/tenant/domain-pools/%i/domains" domainPoolId) data
                do! ensureSuccessAsync result
                let! result = readAsJsonAsync<int> result
                domainId <- Some(result)
            }

        [<Fact>]
        [<Priority(2)>]
        member __.``B Update domain pool must be success``() =
            let domainPoolId = testContext.Value.GetTenant().DomainPoolId
            task {
                let data: PostLike =
                    { EnvName = "Dev" }
                let! result = testFixture.HttpPutAsync userToken
                                  (sprintf "/tenant/domain-pools/%i/domains/%i" domainPoolId domainId.Value) data
                do! ensureSuccessAsync result
            }

        [<Fact>]
        [<Priority(3)>]
        member __.``C Get domain must be success``() =
            let domainPoolId = testContext.Value.GetTenant().DomainPoolId
            task {
                let expected: PostLike =
                    { EnvName = "Dev" }
                let! result = testFixture.HttpGetAsync userToken
                                  (sprintf "/tenant/domain-pools/%i/domains/%i" domainPoolId domainId.Value)
                do! ensureSuccessAsync result
                let! result = readAsJsonAsync<GetLikeDto> result
                result |> should be (not' null)
                result.id |> should equal domainId.Value
                result.envName |> should equal expected.EnvName
                result.dateCreated |> should be (not' null)
            }

        [<Fact>]
        [<Priority(4)>]
        member __.``D Delete domain must be success``() =
            let domainPoolId = testContext.Value.GetTenant().DomainPoolId
            task {
                let! result = testFixture.HttpGetAsync userToken
                                  (sprintf "/tenant/domain-pools/%i/domains/%i" domainPoolId domainId.Value)
                do! ensureSuccessAsync result }
