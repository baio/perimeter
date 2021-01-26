namespace PRR.API.Tests.Tenants

open Akkling
open Common.Test.Utils
open Common.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FSharpx
open FsUnit
open PRR.API.Routes.Tenant
open PRR.API.Tests.Utils
open PRR.Data.Entities
open PRR.Domain.Auth
open PRR.Domain.Auth.SignUp
open PRR.Domain.Tenant
open PRR.Domain.Tenant.DomainUserRoles
open PRR.Domain.Tenant.UserDomains
open TaskUtils
open Xunit
open Xunit.Abstractions
open Xunit.Priority
open FsUnit

// 1. Signup user1 (with tenant1)
// 2. Add user2 as tenant admin for tenant1
// 3. Signup user2 (no tenant)
// 4. Get user2 domains should return tenant1 management domain
// 5. User2 can create domains for tenant1
module MultiTenantAdminTests =

    let user1Data: Data =
        { FirstName = "user1"
          LastName = "XXX"
          Email = "user1@user.com"
          Password = "#6VvR&^"
          QueryString = null }

    let user2Data: Data =
        { FirstName = "user2"
          LastName = "YYY"
          Email = "user2@user.com"
          Password = "#6VvR&^"
          QueryString = null }

    let private users =
        System.Collections.Generic.List<_>
            [ {| Data = user1Data
                 Token = None
                 Tenant = None |}
              {| Data = user2Data
                 Token = None
                 Tenant = None |} ]

    let mutable testContext: UserTestContext option = None

    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``multi-tenant-admin-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.``0 BEFORE ALL``() =
            task {

                // 1. Signup user1 (with tenant1)
                testContext <- Some(createUserTestContext testFixture)
                // create user 1 + tenant
                let u1 = users.[0]

                let! userToken = createUser' false testContext.Value u1.Data

                let tenant = testContext.Value.GetTenant()

                users.[0] <- {| u1 with
                                    Token = Some(userToken)
                                    Tenant = Some(tenant) |}

                // 2. Add user2 as tenant admin for tenant1
                let data: PostLike =
                    { UserEmail = users.[1].Data.Email
                      RolesIds = [ PRR.Data.DataContext.Seed.Roles.TenantAdmin.Id ] }

                let u1 = users.[0]

                let! _ =
                    testFixture.HttpPostAsync u1.Token.Value (sprintf "/api/tenants/%i/admins" tenant.TenantId) data

                return ()
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``A Create user2 no tenant must be success``() =
            // 3. Signup user2 (no tenant)
            task {
                let u2 = users.[1]
                let! token = createUserNoTenant testContext.Value u2.Data
                token |> should be (not' Empty)
                printfn "!!! %s" token
                users.[1] <- {| u2 with Token = Some token |}
            }

        [<Fact>]
        [<Priority(2)>]
        member __.``B Get user2 domains should return tenant1 management domain``() =
            // 4. Get user2 domains should return tenant1 management domain
            task {
                let u2 = users.[1]
                let! result = testFixture.HttpGetAsync u2.Token.Value "/api/me/management/domains"
                do! ensureSuccessAsync result

                let! data = result |> readAsJsonAsync<TenantDomain []>

                printfn "%A" data

                data.Length |> should equal 1
            }

        [<Fact>]
        [<Priority(3)>]
        member __.``C User2 can create domains for tenant1``() =
            // 4. Get user2 domains should return tenant1 management domain
            task {
                let data: DomainPools.PostLike =
                    { Name = "domain 1"
                      Identifier = "domain-1" }

                let info: CreatedTenantInfo = users.[0].Tenant.Value

                let! result =
                    testFixture.HttpPostAsync
                        users.[1].Token.Value
                        (sprintf "/api/tenants/%i/domain-pools" info.TenantId)
                        data

                do! ensureSuccessAsync result
            }

        [<Fact>]
        [<Priority(4)>]
        member __.``D Get user2 domains should return tenant1 and created domain``() =
            task {
                let u2 = users.[1]
                let! result = testFixture.HttpGetAsync u2.Token.Value "/api/me/management/domains"
                do! ensureSuccessAsync result

                let! data = result |> readAsJsonAsync<TenantDomain []>

                printfn "%A" data

                data.Length |> should equal 2
            }


        [<Fact>]
        [<Priority(5)>]
        member __.``E User1 add user2 as domain admin for sample domain``() =
            task {

                let u1 = users.[0]

                // relogin under domain admin first
                let! loginResult =
                    logInUser
                        testFixture
                        u1.Tenant.Value.DomainManagementApplicationClientId
                        u1.Data.Email
                        u1.Data.Password

                let data: PostLike =
                    { UserEmail = users.[1].Data.Email
                      RolesIds = [ PRR.Data.DataContext.Seed.Roles.DomainAdmin.Id ] }
                
                let! result =
                    testFixture.HttpPostAsync
                        loginResult.access_token
                        (sprintf "/api/tenant/domains/%i/users" u1.Tenant.Value.DomainId)
                        data

                do! ensureSuccessAsync result
            }


        [<Fact>]
        [<Priority(6)>]
        member __.``F Get user2 domains should return tenant1 and sample1 management domain``() =
            task {
                let u2 = users.[1]
                let! result = testFixture.HttpGetAsync u2.Token.Value "/api/me/management/domains"
                do! ensureSuccessAsync result

                let! data = result |> readAsJsonAsync<TenantDomain []>

                printfn "%A" data

                data.Length |> should equal 3
            }
