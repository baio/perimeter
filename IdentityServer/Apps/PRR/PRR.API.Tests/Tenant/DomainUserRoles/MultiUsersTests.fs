namespace PRR.API.Tests.Tenant.DomainUserRoles

open Akkling
open Common.Test.Utils
open Common.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FSharpx
open FsUnit
open PRR.API
open PRR.API.Routes.Tenant
open PRR.API.Tests.Utils
open PRR.Data.Entities
open PRR.Domain.Auth
open PRR.Domain.Auth.SignUp
open PRR.Domain.Tenant.DomainUserRoles
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
    type ``domain-user-roles-multi-users-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.``0 BEFORE ALL``() =
            task {
                let testPermission: PRR.Domain.Tenant.Permissions.PostLike =
                    { Name = "test:permission"
                      Description = "test description" }

                let testRole: PRR.Domain.Tenant.Roles.PostLike =
                    { Name = "test:role"
                      Description = "test description"
                      PermissionIds = [] }

                testContext <- Some(createUserTestContext testFixture)
                // create user 1 + tenant + permission
                let u = users.[0]
                let! userToken = createUser testContext.Value u.Data
                let tenant = testContext.Value.GetTenant()
                let! permissionId = (testFixture.HttpPostAsync userToken
                                         (sprintf "/api/tenant/apis/%i/permissions" tenant.SampleApiId)
                                         { testPermission with Name = "test:permission:1" }) >>= (readAsJsonAsync<int>)
                let! roleId = testFixture.HttpPostAsync userToken
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
        member __.``A admin add role 1 to new email should success``() =
            let u1 = users.[0]
            task {
                let data: PostLike =
                    { UserEmail = newUserEmail
                      RolesIds = [ u1.RoleId.Value ] }
                let! result = testFixture.HttpPostAsync u1.Token.Value
                                  (sprintf "/api/tenant/domains/%i/users" u1.Tenant.Value.DomainId) data
                do! ensureSuccessAsync result
            }

        [<Fact>]
        [<Priority(2)>]
        member __.``B user 1 add user 2 with custom role to own tenant should success``() =
            let u1 = users.[0]
            let u2 = users.[1]
            task {
                let data: PostLike =
                    { UserEmail = u2.Data.Email
                      RolesIds = [ u1.RoleId.Value ] }
                let! result = testFixture.HttpPostAsync u1.Token.Value
                                  (sprintf "/api/tenant/domains/%i/users" u1.Tenant.Value.DomainId) data
                do! ensureSuccessAsync result
            }

        [<Fact>]
        [<Priority(3)>]
        member __.``C re-signin 2nd user under 1st tenant should be success``() =
            let u1 = users.[0]
            let u2 = users.[1]
            task {

                let! res = logInUser testFixture u1.Tenant.Value.SampleApplicationClientId u2.Data.Email
                               u2.Data.Password

                res |> should be (not' null)

                res.access_token |> should be (not' null)

                users.[1] <- {| u2 with Token = Some res.access_token |}
            }

        [<Fact>]
        [<Priority(3)>]
        member __.``D user 2 can't add any role to tenant 1``() =
            let u1 = users.[0]
            let u2 = users.[1]
            task {
                let data: PostLike =
                    { UserEmail = "other@mail.com"
                      RolesIds = [ u1.RoleId.Value ] }
                let! result = testFixture.HttpPostAsync u2.Token.Value
                                  (sprintf "/api/tenant/domains/%i/users" u1.Tenant.Value.DomainId) data
                ensureForbidden result
            }

        [<Fact>]
        [<Priority(4)>]
        member __.``E user 1 update user 2 with admin role for own tenant should success``() =
            let u1 = users.[0]
            let u2 = users.[1]

            task {
                let data: PostLike =
                    { UserEmail = u2.Data.Email
                      RolesIds = [ PRR.Data.DataContext.Seed.Roles.DomainAdmin.Id ] }
                let! result = testFixture.HttpPostAsync u1.Token.Value
                                  (sprintf "/api/tenant/domains/%i/users" u1.Tenant.Value.DomainId) data
                do! ensureSuccessAsync result
            }

        [<Fact>]
        [<Priority(5)>]
        member __.``F re-signin 2nd user under 1st tenant should be success``() =
            let u1 = users.[0]
            let u2 = users.[1]
            task {
                let! res = logInUser testFixture u1.Tenant.Value.DomainManagementApplicationClientId u2.Data.Email
                               u2.Data.Password

                res |> should be (not' null)

                res.access_token |> should be (not' null)

                users.[1] <- {| u2 with Token = Some res.access_token |}
            }


        [<Fact>]
        [<Priority(6)>]
        member __.``G user 2 can add new user with custom role to tenant 1``() =
            let u1 = users.[0]
            let u2 = users.[1]
            task {
                let data: PostLike =
                    { UserEmail = "other@mail.com"
                      RolesIds = [ u1.RoleId.Value ] }                    
                let! result = testFixture.HttpPostAsync u2.Token.Value
                                  (sprintf "/api/tenant/domains/%i/users" u1.Tenant.Value.DomainId) data
                do! ensureSuccessAsync result
            }

        [<Fact>]
        [<Priority(7)>]
        member __.``H user 2 forbidden to add new user with admin role to tenant 1``() =
            let u1 = users.[0]
            let u2 = users.[1]
            task {
                let data: PostLike =
                    { UserEmail = "other1@mail.com"
                      RolesIds = [ PRR.Data.DataContext.Seed.Roles.DomainAdmin.Id ] }
                let! result = testFixture.HttpPostAsync u2.Token.Value
                                  (sprintf "/api/tenant/domains/%i/users" u1.Tenant.Value.DomainId) data
                do ensureForbidden result
            }

        [<Fact>]
        [<Priority(8)>]
        member __.``I user 1 update user 2 with super-admin role for own tenant should success``() =
            let u1 = users.[0]
            let u2 = users.[1]

            task {
                let data: PostLike =
                    { UserEmail = u2.Data.Email
                      RolesIds = [ PRR.Data.DataContext.Seed.Roles.DomainSuperAdmin.Id ] }
                let! result = testFixture.HttpPostAsync u1.Token.Value
                                  (sprintf "/api/tenant/domains/%i/users" u1.Tenant.Value.DomainId) data
                do! ensureSuccessAsync result
            }

        [<Fact>]
        [<Priority(9)>]
        member __.``J re-signin 2nd user under 1st tenant should be success``() =
            let u1 = users.[0]
            let u2 = users.[1]
            task {
                let! res = logInUser testFixture u1.Tenant.Value.DomainManagementApplicationClientId u2.Data.Email
                               u2.Data.Password

                // re-signin 2nd tenant under 1st client

                res |> should be (not' null)

                res.access_token |> should be (not' null)

                users.[1] <- {| u2 with Token = Some res.access_token |}
            }


        [<Fact>]
        [<Priority(10)>]
        member __.``K user 2 can add new user with admin role to tenant 1``() =
            let u1 = users.[0]
            let u2 = users.[1]
            task {
                let data: PostLike =
                    { UserEmail = "other-admin@mail.com"
                      RolesIds = [ PRR.Data.DataContext.Seed.Roles.DomainAdmin.Id ] }
                let! result = testFixture.HttpPostAsync u2.Token.Value
                                  (sprintf "/api/tenant/domains/%i/users" u1.Tenant.Value.DomainId) data
                do! ensureSuccessAsync result
            }


        [<Fact>]
        [<Priority(11)>]
        member __.``L remove domain owner should give error``() =
            let u1 = users.[0]
            task {
                let! result = testFixture.HttpDeleteAsync u1.Token.Value
                                  (sprintf "/api/tenant/domains/%i/users/%s" u1.Tenant.Value.DomainId u1.Data.Email)
                do ensureForbidden result }

        [<Fact>]
        [<Priority(12)>]
        member __.``M update domain owner should give error``() =
            let u1 = users.[0]
            task {
                let data: PostLike =
                    { UserEmail = u1.Data.Email
                      RolesIds = [ PRR.Data.DataContext.Seed.Roles.DomainAdmin.Id ] }
                let! result = testFixture.HttpPostAsync u1.Token.Value
                                  (sprintf "/api/tenant/domains/%i/users" u1.Tenant.Value.DomainId) data
                do ensureForbidden result
            }

        [<Fact>]
        [<Priority(13)>]
        member __.``N trying to create user with wrong email should give bas request error``() =
            let u1 = users.[0]
            task {
                let data: PostLike =
                    { UserEmail = "test"
                      RolesIds = [ PRR.Data.DataContext.Seed.Roles.DomainAdmin.Id ] }
                let! result = testFixture.HttpPostAsync u1.Token.Value
                                  (sprintf "/api/tenant/domains/%i/users" u1.Tenant.Value.DomainId) data
                do ensureBadRequest result
                let! result' = readAsJsonAsync<ErrorDataDTO<Map<string, string array>>> result

                let expected =
                    [ ("userEmail", [| "CUSTOM:The UserEmail field is not a valid e-mail address." |]) ] |> Map
                    
                result'.Data |> should equal expected                                
                
            }
