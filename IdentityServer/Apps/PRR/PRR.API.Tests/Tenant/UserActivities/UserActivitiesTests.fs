namespace PRR.API.Tests.UserActivities

open System.Threading
open DataAvail.Test.Common
open DataAvail.ListQuery.Core.Models
open FSharp.Control.Tasks.V2.ContextInsensitive
open FsUnit
open PRR.API.Tests.Utils
open PRR.Domain.Auth.SignUp
//open PRR.System.Views.LogInView
open PRR.Domain.Common.Events
open PRR.Domain.Tenant.Views.LogInView
open Xunit
open Xunit.Abstractions
open Xunit.Priority
//open PRR.System.Views.LogInView

module CRUD = 

    let userData: Data =
        { FirstName = "First"
          LastName = "Last"
          Email = "user@user.com"
          Password = "#6VvR&^"
          ReturnUrl = null }

    let userPassword = "123"

    let mutable userToken: string = null

    let mutable testContext: UserTestContext option = None

    let mutable domainPoolId: int option = None

    [<CLIMutable>]
    type GetLikeDomainDto =
        { id: int
          name: string
          dateCreated: System.DateTime }

    [<CLIMutable>]
    type GetLikeDto =
        { id: int
          name: string
          dateCreated: System.DateTime
          domains: GetLikeDomainDto seq }

    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``user-activities-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.``0 BEFORE ALL``() =
            task {
                testContext <- Some(createUserTestContext testFixture)
                let! userToken' = createUser' true testContext.Value userData
                userToken <- userToken'                
                // test server must be started before make any request, in order to bus can receive message !
                let! _ = testFixture.Server2.HttpGetAsync' "/api/auth/version"
                Thread.Sleep(100)
                ()
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``A Get user activities must be success``() =            
            task {
                               
                let tenant = testContext.Value.GetTenant()

                let! result =
                    testFixture.Server2.HttpGetAsync
                        userToken
                        (sprintf "/api/tenant/domains/%i/admin-activities" tenant.DomainId)

                do! ensureSuccessAsync result

                let! resultDto = readAsJsonAsync<ListQueryResult<LogInDoc>> result

                resultDto.Items |> should haveCount 1

                ()
            }

        [<Fact>]
        [<Priority(2)>]
        member __.``B Get user activities with filter give correct result``() =
            task {

                let tenant = testContext.Value.GetTenant()

                let! result =
                    testFixture.Server2.HttpGetAsync
                        userToken
                        (sprintf "/api/tenant/domains/%i/admin-activities?filter.email=xxx" tenant.DomainId)

                do! ensureSuccessAsync result

                let! resultDto = readAsJsonAsync<ListQueryResult<LogInDoc>> result

                resultDto.Items |> should haveCount 0

                ()
            }


        [<Fact>]
        [<Priority(2)>]
        member __.``C Get user activities with existent filter give correct result``() =
            task {

                let tenant = testContext.Value.GetTenant()

                let! result =
                    testFixture.Server2.HttpGetAsync
                        userToken
                        (sprintf "/api/tenant/domains/%i/admin-activities?filter.email=user.c" tenant.DomainId)

                do! ensureSuccessAsync result

                let! resultDto = readAsJsonAsync<ListQueryResult<LogInDoc>> result

                resultDto.Items |> should haveCount 1

                ()
            }