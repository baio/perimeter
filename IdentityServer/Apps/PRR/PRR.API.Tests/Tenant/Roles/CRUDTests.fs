namespace PRR.API.Tests.Tenant.Roles

open Common.Test.Utils
open Common.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FsUnit
open PRR.API.Tests.Utils
open PRR.Domain.Auth.SignUp
open PRR.Domain.Tenant
open PRR.Domain.Tenant.Roles
open System
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

    let mutable permissionId1: int option = None
    let mutable permissionId2: int option = None
    let mutable roleId: int option = None


    [<CLIMutable>]
    type PermissionGetLikeDto = { id: int; name: string }

    [<CLIMutable>]
    type GetLikeDto =
        { id: int
          name: string
          description: string
          permissions: PermissionGetLikeDto []
          dateCreated: System.DateTime }

    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``roles-crud-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.``0 Before All``() =
            task {
                testContext <- Some(createUserTestContext testFixture)
                let! userToken' = createUser testContext.Value userData
                userToken <- userToken'

                let data: Permissions.PostLike =
                    { Name = "read:test1"
                      Description = "test description"
                      IsDefault = false }

                let! permissionId' =
                    testFixture.HttpPostAsync
                        userToken
                        (sprintf "/api/tenant/apis/%i/permissions" (testContext.Value.GetTenant().SampleApiId))
                        data
                    >>= readAsJsonAsync<int>

                permissionId1 <- Some permissionId'

                let data: Permissions.PostLike =
                    { Name = "read:test2"
                      Description = "test description"
                      IsDefault = false }

                let! permissionId' =
                    testFixture.HttpPostAsync
                        userToken
                        (sprintf "/api/tenant/apis/%i/permissions" (testContext.Value.GetTenant().SampleApiId))
                        data
                    >>= readAsJsonAsync<int>

                permissionId2 <- Some permissionId'
            }


        [<Fact>]
        [<Priority(1)>]
        member __.``A Create role must be success``() =
            let domainId = testContext.Value.GetTenant().DomainId
            task {
                let data: PostLike =
                    { Name = "role"
                      Description = "role description"
                      PermissionIds = [ permissionId1.Value ] }

                let! result = testFixture.HttpPostAsync userToken (sprintf "/api/tenant/domains/%i/roles" domainId) data
                do! ensureSuccessAsync result
                let! result = readAsJsonAsync<int> result
                roleId <- Some(result)
            }

        [<Fact>]
        [<Priority(2)>]
        member __.``B.1 Create role with same name must Conflict``() =
            let domainId = testContext.Value.GetTenant().DomainId
            task {
                let data: PostLike =
                    { Name = "role"
                      Description = "role description"
                      PermissionIds = [ permissionId1.Value ] }

                let! result = testFixture.HttpPostAsync userToken (sprintf "/api/tenant/domains/%i/roles" domainId) data
                ensureConflict result
            }

        [<Fact>]
        [<Priority(2)>]
        member __.``B.2 Get role must be success``() =
            let domainId = testContext.Value.GetTenant().DomainId
            task {
                let expected: GetLikeDto =
                    { name = "role"
                      description = "role description"
                      permissions =
                          [| { id = permissionId1.Value
                               name = "read:test1" } |]
                      id = -1
                      dateCreated = DateTime.UtcNow }

                let! result =
                    testFixture.HttpGetAsync userToken (sprintf "/api/tenant/domains/%i/roles/%i" domainId roleId.Value)

                do! ensureSuccessAsync result
                let! result = readAsJsonAsync<GetLikeDto> result
                result |> should be (not' null)
                result.id |> should equal roleId.Value
                result.name |> should equal expected.name

                result.description
                |> should equal expected.description

                result.permissions
                |> should equal expected.permissions

                result.dateCreated |> should be (not' null)
            }

        [<Fact>]
        [<Priority(3)>]
        member __.``C Update role must be success``() =
            let apiId = testContext.Value.GetTenant().DomainId
            task {
                let data: PostLike =
                    { Name = "role2"
                      Description = "test description2"
                      PermissionIds = [ permissionId2.Value ] }

                let! result =
                    testFixture.HttpPutAsync
                        userToken
                        (sprintf "/api/tenant/domains/%i/roles/%i" apiId roleId.Value)
                        data

                do! ensureSuccessAsync result
            }


        [<Fact>]
        [<Priority(4)>]
        member __.``D Get updated role must be success``() =
            let domainId = testContext.Value.GetTenant().DomainId
            task {
                let expected: GetLikeDto =
                    { name = "role2"
                      description = "test description2"
                      permissions =
                          [| { id = permissionId2.Value
                               name = "read:test2" } |]
                      id = -1
                      dateCreated = DateTime.UtcNow }

                let! result =
                    testFixture.HttpGetAsync userToken (sprintf "/api/tenant/domains/%i/roles/%i" domainId roleId.Value)

                do! ensureSuccessAsync result
                let! result = readAsJsonAsync<GetLikeDto> result
                result |> should be (not' null)
                result.id |> should equal roleId.Value
                result.name |> should equal expected.name

                result.description
                |> should equal expected.description

                result.permissions
                |> should equal expected.permissions

                result.dateCreated |> should be (not' null)
            }

        [<Fact>]
        [<Priority(5)>]
        member __.``E Delete role must be success``() =
            let domainId = testContext.Value.GetTenant().DomainId
            task {
                let! result =
                    testFixture.HttpDeleteAsync
                        userToken
                        (sprintf "/api/tenant/domains/%i/roles/%i" domainId roleId.Value)

                do! ensureSuccessAsync result
            }
