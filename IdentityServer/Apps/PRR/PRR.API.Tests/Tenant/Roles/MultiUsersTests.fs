namespace PRR.API.Tests.Tenant.Roles

open Akkling
open Common.Test.Utils
open Common.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FSharpx
open FsUnit
open PRR.API.Routes.Tenant
open PRR.API.Tests.Utils
open PRR.Data.Entities
open PRR.Domain.Auth.SignUp
open PRR.Domain.Tenant.Roles
open PRR.System.Models
open TaskUtils
open Xunit
open Xunit.Abstractions
open Xunit.Priority

module MultiUsers =

    let user1Data: Data =
        { FirstName = "First"
          LastName = "XXX"
          Email = "user1@user.com"
          Password = "#6VvR&^"
          QueryString = null }

    let user2Data: Data =
        { FirstName = "Second"
          LastName = "YYY"
          Email = "user2@user.com"
          Password = "#6VvR&^"
          QueryString = null }

    let private users =
        System.Collections.Generic.List<_>
            [ {| Data = user1Data
                 Token = None
                 Tenant = None
                 PermissionId = None |}
              {| Data = user2Data
                 Token = None
                 Tenant = None
                 PermissionId = None |} ]

    let mutable testContext: UserTestContext option = None


    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``roles-multi-users-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.BeforeAll() =
            task {
                let testPermission: PRR.Domain.Tenant.Permissions.PostLike =
                    { Name = "test:permissions"
                      Description = "test description"
                      IsDefault = false }

                testContext <- Some(createUserTestContext testFixture)
                // create user 1 + tenant + permission
                let u = users.[0]

                let! userToken = createUser testContext.Value u.Data
                let tenant = testContext.Value.GetTenant()

                let! permissionId =
                    (testFixture.HttpPostAsync
                        userToken
                         (sprintf "/api/tenant/apis/%i/permissions" tenant.SampleApiId)
                         { testPermission with
                               Name = "test:permission:1" })
                    >>= (readAsJsonAsync<int>)

                users.[0] <- {| u with
                                    Token = Some(userToken)
                                    Tenant = Some(tenant)
                                    PermissionId = Some(permissionId) |}

                // create user 2 + tenant + permission
                let u = users.[1]

                let! userToken = createUser testContext.Value u.Data
                let tenant = testContext.Value.GetTenant()

                let! permissionId =
                    testFixture.HttpPostAsync
                        userToken
                        (sprintf "/api/tenant/apis/%i/permissions" tenant.SampleApiId)
                        { testPermission with
                              Name = "test:permission:2" }
                    >>= (readAsJsonAsync<int>)

                users.[1] <- {| u with
                                    Token = Some(userToken)
                                    Tenant = Some(tenant)
                                    PermissionId = Some(permissionId) |}
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``user 1 forbidden to create roles with permissions from user 2 tenant``() =
            let u1 = users.[0]
            let u2 = users.[1]
            task {
                let data: PostLike =
                    { Name = "test:permissions"
                      Description = "test description"
                      PermissionIds = [ u2.PermissionId.Value ] }

                let! result =
                    testFixture.HttpPostAsync
                        u1.Token.Value
                        (sprintf "/api/tenant/domains/%i/roles" u1.Tenant.Value.DomainId)
                        data

                ensureForbidden result
            }
