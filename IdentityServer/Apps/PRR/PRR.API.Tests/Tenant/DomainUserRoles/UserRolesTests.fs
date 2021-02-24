namespace PRR.API.Tests.Tenant.DomainUserRoles

open DataAvail.Test.Common
open FSharp.Control.Tasks.V2.ContextInsensitive
open FsUnit
open PRR.API
open PRR.API.Common.ErrorHandler
open PRR.API.Tests.Utils
open PRR.Domain.Auth.SignUp
open PRR.Domain.Tenant.DomainUserRoles
open Xunit
open Xunit.Abstractions
open Xunit.Priority
open DataAvail.Common.TaskUtils

module UserRoles =

    let user1Data: Data =
        { FirstName = "First"
          LastName = "XXX"
          Email = "user1@user.com"
          Password = "#6VvR&^"
          ReturnUrl = null }

    let user2Data: Data =
        { FirstName = "Second"
          LastName = "YYY"
          Email = "user2@user.com"
          Password = "#6VvR&^"
          ReturnUrl = null }

    let newUserEmail = "new@user.com"

    let private users =
        System.Collections.Generic.List<_>
            [ {| Data = user1Data
                 Token = None
                 Tenant = None
                 PermissionId = None
                 RoleId = None |}
              {| Data = user2Data
                 Token = None
                 Tenant = None
                 PermissionId = None
                 RoleId = None |} ]

    let mutable testContext: UserTestContext option = None

    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``domain-user-roles-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.``0 BEFORE ALL``() =
            task {
                let testPermission: PRR.Domain.Tenant.Permissions.PostLike =
                    { Name = "test:permission"
                      Description = "test description"
                      IsDefault = false }

                let testRole: PRR.Domain.Tenant.Roles.PostLike =
                    { Name = "test:role"
                      Description = "test description"
                      PermissionIds = [] }

                testContext <- Some(createUserTestContext testFixture)
                // create user 1 + tenant + permission
                let u = users.[0]

                let! userToken = createUser testContext.Value u.Data
                let tenant = testContext.Value.GetTenant()

                let! permissionId =
                    (testFixture.Server2.HttpPostAsync
                        userToken
                         (sprintf "/api/tenant/apis/%i/permissions" tenant.SampleApiId)
                         { testPermission with
                               Name = "test:permission:1" })
                    >>= (readAsJsonAsync<int>)

                let! roleId =
                    testFixture.Server2.HttpPostAsync
                        userToken
                        (sprintf "/api/tenant/domains/%i/roles" tenant.DomainId)
                        { testRole with
                              Name = "test:role:1"
                              PermissionIds = [ permissionId ] }
                    >>= (readAsJsonAsync<int>)

                users.[0] <- {| u with
                                    Token = Some(userToken)
                                    Tenant = Some(tenant)
                                    PermissionId = Some(permissionId)
                                    RoleId = Some(roleId) |}

                // create user 2 + tenant + permission
                let u = users.[1]

                let! userToken = createUser testContext.Value u.Data
                let tenant = testContext.Value.GetTenant()

                users.[1] <- {| u with
                                    Token = Some(userToken)
                                    Tenant = Some(tenant)
                                    PermissionId = Some(permissionId)
                                    RoleId = Some(roleId) |}
            }


        [<Fact>]
        [<Priority(1)>]
        member __.``A admin add role 1 to itself should success``() =
            let u1 = users.[0]
            task {
                let data: PostLike =
                    { UserEmail = u1.Data.Email
                      RolesIds = [ u1.RoleId.Value ] }

                let! result =
                    testFixture.Server2.HttpPostAsync
                        u1.Token.Value
                        (sprintf "/api/tenant/domains/%i/users" u1.Tenant.Value.DomainId)
                        data

                do! ensureSuccessAsync result
            }

