namespace PRR.API.Tests.Tenant.DomainPools

open DataAvail.Test.Common
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.API.Tests.Utils
open PRR.Domain.Auth.SignUp
open PRR.Domain.Tenant.DomainPools
open Xunit
open Xunit.Abstractions
open Xunit.Priority

module MultiUsers =

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
    type ``domain-pools-multi-users-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.``0 BEFORE ALL``() =
            task {
                testContext <- Some(createUserTestContext testFixture)
                // create user 1 + tenant + permission
                let u = users.[0]

                let! userToken = createUser testContext.Value u.Data

                let tenant = testContext.Value.GetTenant()

                users.[0] <- {| u with
                                    Token = Some(userToken)
                                    Tenant = Some(tenant) |}

                // create user 2 + tenant + permission
                let u = users.[1]

                let! userToken = createUser testContext.Value u.Data

                let tenant = testContext.Value.GetTenant()

                users.[1] <- {| u with
                                    Token = Some(userToken)
                                    Tenant = Some(tenant) |}
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``A user 2 forbidden to update domain pool of 1 tenant``() =
            let u1 = users.[0]
            let u2 = users.[1]
            task {
                let data: PutLike = { Name = "Domain pool update" }

                let! result =
                    testFixture.Server2.HttpPutAsync
                        u2.Token.Value
                        (sprintf "/api/tenant/tenants/%i/domain-pools/%i" u1.Tenant.Value.TenantId u1.Tenant.Value.DomainPoolId)
                        data

                ensureForbidden result
            }

        [<Fact>]
        [<Priority(2)>]
        member __.``B user 2 forbidden to update domain pool of 1 tenant``() =
            let u1 = users.[0]
            let u2 = users.[1]
            task {
                let data: PostLike =
                    { Name = "New domain pool"
                      Identifier = "n dom" }

                let! result =
                    testFixture.Server2.HttpPostAsync
                        u2.Token.Value
                        (sprintf "/api/tenant/tenants/%i/domain-pools" u1.Tenant.Value.TenantId)
                        data

                ensureForbidden result
            }
