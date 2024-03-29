﻿namespace PRR.API.Tests.Tenant.Apis
open DataAvail.Test.Common
open DataAvail.Common.TaskUtils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FsUnit
open PRR.API.Tests.Utils
open PRR.Domain.Auth.SignUp
open PRR.Domain.Tenant
open PRR.Domain.Tenant.Apis
open Xunit
open Xunit.Abstractions
open Xunit.Priority

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

    let mutable permissionId1: int option = None
    let mutable permissionId2: int option = None
    let mutable apiId: int option = None

    [<CLIMutable>]
    type PermissionGetLikeDto = { id: int; name: string }

    [<CLIMutable>]
    type GetLikeDto =
        { id: int
          name: string
          identifier: string
          permissions: PermissionGetLikeDto []
          accessTokenExpiresIn: int
          dateCreated: System.DateTime }

    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``apis-crud-api``(testFixture: TestFixture, output: ITestOutputHelper) =
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
                    testFixture.Server2.HttpPostAsync
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
                    testFixture.Server2.HttpPostAsync
                        userToken
                        (sprintf "/api/tenant/apis/%i/permissions" (testContext.Value.GetTenant().SampleApiId))
                        data
                    >>= readAsJsonAsync<int>

                permissionId2 <- Some permissionId'
            }


        [<Fact>]
        [<Priority(1)>]
        member __.``A Create api must be success``() =
            let domainId = testContext.Value.GetTenant().DomainId
            task {
                let data: PostLike = { Name = "Api 1"; Identifier = "xxx" }

                let! result = testFixture.Server2.HttpPostAsync userToken (sprintf "/api/tenant/domains/%i/apis" domainId) data

                do! ensureSuccessAsync result

                let! result = readAsJsonAsync<int> result
                apiId <- Some(result)
            }

        [<Fact>]
        [<Priority(2)>]
        member __.``B Get api must be success``() =
            let domainId = testContext.Value.GetTenant().DomainId
            task {
                let! result =
                    testFixture.Server2.HttpGetAsync userToken (sprintf "/api/tenant/domains/%i/apis/%i" domainId apiId.Value)

                do! ensureSuccessAsync result

                let! result = readAsJsonAsync<GetLikeDto> result

                result |> should be (not' null)
                result.id |> should equal apiId.Value
                result.name |> should equal "Api 1"
                result.permissions |> should equal [||]
                result.dateCreated |> should be (not' null)
            }

        [<Fact>]
        [<Priority(3)>]
        member __.``C Update api must be success``() =
            let domainId = testContext.Value.GetTenant().DomainId
            task {
                let data: PostLike =
                    { Name = "Api 1 Updated"
                      Identifier = "yyy" }

                let! result =
                    testFixture.Server2.HttpPutAsync
                        userToken
                        (sprintf "/api/tenant/domains/%i/apis/%i" domainId apiId.Value)
                        data

                do! ensureSuccessAsync result
            }


        [<Fact>]
        [<Priority(4)>]
        member __.``D Get updated api must be success``() =
            let domainId = testContext.Value.GetTenant().DomainId
            task {
                let! result =
                    testFixture.Server2.HttpGetAsync userToken (sprintf "/api/tenant/domains/%i/apis/%i" domainId apiId.Value)

                do! ensureSuccessAsync result

                let! result = readAsJsonAsync<GetLikeDto> result

                result |> should be (not' null)
                result.id |> should equal apiId.Value
                result.name |> should equal "Api 1 Updated"
                result.permissions |> should equal [||]
                result.dateCreated |> should be (not' null)
            }

        [<Fact>]
        [<Priority(5)>]
        member __.``E Delete role must be success``() =
            let domainId = testContext.Value.GetTenant().DomainId
            task {
                let! result =
                    testFixture.Server2.HttpDeleteAsync
                        userToken
                        (sprintf "/api/tenant/domains/%i/apis/%i" domainId apiId.Value)

                do! ensureSuccessAsync result
            }
