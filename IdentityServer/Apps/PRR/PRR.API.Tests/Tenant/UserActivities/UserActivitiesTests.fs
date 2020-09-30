namespace PRR.API.Tests.UserActivities

open System.Threading
open Akkling
open Common.Test.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FsUnit
open PRR.API.Tests.Utils
open PRR.Domain.Auth.SignUp
open PRR.System.Views.LogInView
open PRR.System.Models
open Xunit
open Xunit.Abstractions
open Xunit.Priority
open PRR.System.Views.LogInView

module CRUD =

    let userData: Data =
        { FirstName = "First"
          LastName = "Last"
          Email = "user@user.com"
          Password = "#6VvR&^"
          QueryString = null }

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
                printf "%s" userToken
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``A Get user activities must be success``() =
            task {
                
                Thread.Sleep(100)
                
                let tenant = testContext.Value.GetTenant()
                               
                let! result =
                    testFixture.HttpGetAsync
                        userToken
                        (sprintf "/api/tenant/domains/%i/admin-activities" tenant.DomainId)                
                do! ensureSuccessAsync result

                let! resultDto = readAsJsonAsync<ListResponse> result

                resultDto.Items |> should haveCount 1

                ()
            }
