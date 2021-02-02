namespace PRR.API.Tests.Tenant.Permissions

open DataAvail.Test.Common
open FSharp.Control.Tasks.V2.ContextInsensitive
open FsUnit
open PRR.API.Tests.Utils
open PRR.Domain.Auth.SignUp
open PRR.Domain.Tenant.Permissions
open Xunit
open Xunit.Abstractions
open Xunit.Priority

module CRUD =

    let userData: Data =
        { FirstName = "First"
          LastName = "Last"
          Email = "user@user.com"
          Password = "#6VvR&^"
          QueryString = null }

    let mutable userToken: string = null

    let mutable testContext: UserTestContext option = None

    let mutable permissionId: int option = None


    [<CLIMutable>]
    type GetLikeDto =
        { id: int
          name: string
          description: string
          dateCreated: System.DateTime }

    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``permissions-crud-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.``0 BeforeAll``() =
            task {
                testContext <- Some(createUserTestContext testFixture)

                let! userToken' = createUser testContext.Value userData
                userToken <- userToken'
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``A Create permission must be success``() =
            let apiId =
                testContext.Value.GetTenant().SampleApiId

            task {
                let data: PostLike =
                    { Name = "test:permissions"
                      Description = "test description"
                      IsDefault = false }

                let! result =
                    testFixture.Server2.HttpPostAsync userToken (sprintf "/api/tenant/apis/%i/permissions" apiId) data

                do! ensureSuccessAsync result
                let! result = readAsJsonAsync<int> result
                permissionId <- Some(result)
            }

        [<Fact>]
        [<Priority(2)>]
        member __.``B Create permission with the same name must fail``() =
            let apiId =
                testContext.Value.GetTenant().SampleApiId

            task {
                let data: PostLike =
                    { Name = "test:permissions"
                      Description = "test description"
                      IsDefault = false }

                let! result =
                    testFixture.Server2.HttpPostAsync userToken (sprintf "/api/tenant/apis/%i/permissions" apiId) data

                do ensureConflict result
            }


        [<Fact>]
        [<Priority(3)>]
        member __.``C Update permission must be success``() =
            let apiId =
                testContext.Value.GetTenant().SampleApiId

            task {
                let data: PostLike =
                    { Name = "test:permissions2"
                      Description = "test description2"
                      IsDefault = false }

                let! result =
                    testFixture.Server2.HttpPutAsync
                        userToken
                        (sprintf "/api/tenant/apis/%i/permissions/%i" apiId permissionId.Value)
                        data

                do! ensureSuccessAsync result
            }

        [<Fact>]
        [<Priority(4)>]
        member __.``D Get permission must be success``() =
            let apiId =
                testContext.Value.GetTenant().SampleApiId

            task {
                let expected: PostLike =
                    { Name = "test:permissions2"
                      Description = "test description2"
                      IsDefault = false }

                let! result =
                    testFixture.Server2.HttpGetAsync
                        userToken
                        (sprintf "/api/tenant/apis/%i/permissions/%i" apiId permissionId.Value)

                do! ensureSuccessAsync result
                let! result = readAsJsonAsync<GetLikeDto> result
                result |> should be (not' null)
                result.id |> should equal permissionId.Value
                result.name |> should equal expected.Name

                result.description
                |> should equal expected.Description

                result.dateCreated |> should be (not' null)
            }

        [<Fact>]
        [<Priority(5)>]
        member __.``E Delete permission must be success``() =
            let apiId =
                testContext.Value.GetTenant().SampleApiId

            task {
                let! result =
                    testFixture.Server2.HttpDeleteAsync
                        userToken
                        (sprintf "/api/tenant/apis/%i/permissions/%i" apiId permissionId.Value)

                do! ensureSuccessAsync result
            }
