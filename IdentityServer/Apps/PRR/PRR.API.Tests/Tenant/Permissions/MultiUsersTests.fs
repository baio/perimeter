namespace PRR.API.Tests.Tenant.Permssions

open Akkling
open Common.Test.Utils
open Common.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FSharpx
open FsUnit
open PRR.API.Tests.Utils
open PRR.Data.Entities
open PRR.Domain.Auth.SignUp
open PRR.Domain.Tenant.Permissions
open PRR.System.Models
open Xunit
open Xunit.Abstractions
open Xunit.Priority
open TaskUtils

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

    [<CLIMutable>]
    type GetLikeDto =
        { id: int
          name: string
          description: string
          dateCreated: System.DateTime }

    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``permissions-multi-users-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.BeforeAll() =
            task {
                let testPermission: PostLike =
                    { Name = "test:permissions"
                      Description = "test description" }
                testContext <- Some(createUserTestContext testFixture)
                // create user 1 + tenant + permission
                let u = users.[0]
                let! userToken = createUser testContext.Value u.Data
                let tenant = testContext.Value.GetTenant()
                let! permissionId = (testFixture.HttpPostAsync userToken
                                        (sprintf "/api/tenant/apis/%i/permissions" tenant.SampleApiId)
                                        { testPermission with Name = "test:permission:1" }) >>= (readAsJsonAsync<int>)
                users.[0] <- {| u with
                                    Token = Some(userToken)
                                    Tenant = Some(tenant)
                                    PermissionId = Some(permissionId) |}
                                                                        
                // create user 2 + tenant + permission
                let u = users.[1]
                let! userToken = createUser testContext.Value u.Data
                let tenant = testContext.Value.GetTenant()
                let! permissionId = testFixture.HttpPostAsync userToken
                                        (sprintf "/api/tenant/apis/%i/permissions" tenant.SampleApiId)
                                        { testPermission with Name = "test:permission:2" } >>= (readAsJsonAsync<int>)
                users.[1] <- {| u with
                                    Token = Some(userToken)
                                    Tenant = Some(tenant)
                                    PermissionId = Some(permissionId) |}                                    
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``user 1 forbidden to create permission in user 2 tenant``() =
            let u1 = users.[0]
            let u2 = users.[1]
            task {
                let data: PostLike =
                    { Name = "test:permissions"
                      Description = "test description" }
                let! result = testFixture.HttpPostAsync u1.Token.Value (sprintf "/api/tenant/apis/%i/permissions" u2.Tenant.Value.SampleApiId) data
                ensureForbidden result
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``user 1 forbidden to update permission in user 2 tenant``() =
            let u1 = users.[0]
            let u2 = users.[1]
            task {
                let data: PostLike =
                    { Name = "test:permissions"
                      Description = "test description" }
                let! result = testFixture.HttpPutAsync u1.Token.Value (sprintf "/api/tenant/apis/%i/permissions/%i" u2.Tenant.Value.SampleApiId u2.PermissionId.Value) data
                ensureForbidden result
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``user 1 forbidden to get permission in user 2 tenant``() =
            let u1 = users.[0]
            let u2 = users.[1]
            task {
                let! result = testFixture.HttpGetAsync u1.Token.Value (sprintf "/api/tenant/apis/%i/permissions/%i" u2.Tenant.Value.SampleApiId u2.PermissionId.Value)
                ensureForbidden result
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``user 1 forbidden to delete permission in user 2 tenant``() =
            let u1 = users.[0]
            let u2 = users.[1]
            task {
                let! result = testFixture.HttpDeleteAsync u1.Token.Value (sprintf "/api/tenant/apis/%i/permissions/%i" u2.Tenant.Value.SampleApiId u2.PermissionId.Value)
                ensureForbidden result
            }
