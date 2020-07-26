namespace PRR.API.Tests.Tenant.Applications

open Akkling
open Common.Test.Utils
open Common.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FsUnit
open PRR.API.Tests.Tenant.Permssions.CRUD
open PRR.API.Tests.Utils
open PRR.Domain.Auth.SignUp
open PRR.Domain.Tenant
open PRR.Domain.Tenant.Applications
open PRR.System.Models
open System
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

    let mutable applicationId: int option = None

    [<CLIMutable>]
    type PermissionGetLikeDto =
        { id: int
          name: string }

    [<CLIMutable>]
    type GetLikeDto =
        { id: int
          name: string
          clientId: string
          clientSecret: string
          dateCreated: DateTime
          idTokenExpiresIn: int
          refreshTokenExpiresIn: int }

    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``applications-crud-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.``0 Before All``() =
            task {
                testContext <- Some(createUserTestContext testFixture)
                let! userToken' = createUser testContext.Value userData userPassword
                userToken <- userToken'
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``A Create application must be success``() =
            let domainId = testContext.Value.GetTenant().DomainId
            task {
                let data: PostLike =
                    { Name = "App 1"
                      ClientId = "xxx" }
                let! result = testFixture.HttpPostAsync userToken (sprintf "/tenant/domains/%i/applications" domainId) data
                do! ensureSuccessAsync result
                let! result = readAsJsonAsync<int> result
                applicationId <- Some(result)
            }

        [<Fact>]
        [<Priority(2)>]
        member __.``B Get app must be success``() =
            let domainId = testContext.Value.GetTenant().DomainId
            task {
                let expected: GetLikeDto =
                    { name = "App 1"
                      clientId = "xxx"
                      id = -1
                      clientSecret = "xxx"
                      dateCreated = DateTime.UtcNow
                      idTokenExpiresIn = 100
                      refreshTokenExpiresIn = 100 }
                let! result = testFixture.HttpGetAsync userToken
                                  (sprintf "/tenant/domains/%i/applications/%i" domainId applicationId.Value)
                do! ensureSuccessAsync result
                let! result = readAsJsonAsync<GetLikeDto> result
                result |> should be (not' null)
                result.id |> should equal applicationId.Value
                result.name |> should equal expected.name
                result.clientId |> should equal expected.clientId
                result.clientSecret |> should be (not' null)
                result.dateCreated |> should be (not' null)
                result.idTokenExpiresIn |> should be (not' null)
                result.refreshTokenExpiresIn |> should be (not' null)
            }

        [<Fact>]
        [<Priority(3)>]
        member __.``C Update app must be success``() =
            let domainId = testContext.Value.GetTenant().DomainId
            task {
                let data: PostLike =
                    { Name = "App 1 Updated"
                      ClientId = "zzz" }
                let! result = testFixture.HttpPutAsync userToken
                                  (sprintf "/tenant/domains/%i/applications/%i" domainId applicationId.Value) data
                do! ensureSuccessAsync result
            }

        [<Fact>]
        [<Priority(4)>]
        member __.``D Get app after update must be success``() =
            let domainId = testContext.Value.GetTenant().DomainId
            task {
                let expected: GetLikeDto =
                    { name = "App 1 Updated"
                      clientId = "zzz"
                      id = -1
                      clientSecret = "xxx"
                      dateCreated = DateTime.UtcNow
                      idTokenExpiresIn = 100
                      refreshTokenExpiresIn = 100 }
                let! result = testFixture.HttpGetAsync userToken
                                  (sprintf "/tenant/domains/%i/applications/%i" domainId applicationId.Value)
                do! ensureSuccessAsync result
                let! result = readAsJsonAsync<GetLikeDto> result
                result |> should be (not' null)
                result.id |> should equal applicationId.Value
                result.name |> should equal expected.name
                result.clientId |> should equal expected.clientId
                result.clientSecret |> should be (not' null)
                result.dateCreated |> should be (not' null)
                result.idTokenExpiresIn |> should be (not' null)
                result.refreshTokenExpiresIn |> should be (not' null)
            }

        [<Fact>]
        [<Priority(5)>]
        member __.``E Delete role must be success``() =
            let domainId = testContext.Value.GetTenant().DomainId
            task {
                let! result = testFixture.HttpDeleteAsync userToken
                                  (sprintf "/tenant/domains/%i/applications/%i" domainId applicationId.Value)
                do! ensureSuccessAsync result }
