namespace PRR.API.Tests.Tenant.Domains

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
open PRR.Domain.Tenant.DomainPools
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

    let user1Password = "123"

    let user2Data: Data =
        { FirstName = "Second"
          LastName = "YYY"
          Email = "user2@user.com"
          Password = "#6VvR&^"
          QueryString = null }

    let user2Password = "123"


    let private users =
        System.Collections.Generic.List<_>
            [ {| Data = user1Data
                 Password = user1Password
                 Token = None
                 Tenant = None |}
              {| Data = user2Data
                 Password = user2Password
                 Token = None
                 Tenant = None |} ]

    let mutable testContext: UserTestContext option = None



    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``domains-multi-users-api``(testFixture: TestFixture, output: ITestOutputHelper) =
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
        member __.``A user 1 forbidden to update domain of 2 tenant``() =
            let u1 = users.[0]
            let u2 = users.[1]
            task {
                let data: PutLike = { Name = "Domain update" }

                let! result =
                    testFixture.HttpPutAsync
                        u2.Token.Value
                        (sprintf
                            "/api/tenant/domain-pools/%i/domains/%i"
                             u1.Tenant.Value.DomainPoolId
                             u1.Tenant.Value.DomainId)
                        data

                ensureForbidden result
            }

        [<Fact>]
        [<Priority(2)>]
        member __.``B user 1 forbidden to create domain of 2 tenant``() =
            let u1 = users.[0]
            let u2 = users.[1]
            task {
                let data: PostLike =
                    { Name = "New Domain"
                      Identifier = "new-dom" }

                let! result =
                    testFixture.HttpPostAsync
                        u2.Token.Value
                        (sprintf "/api/tenant/domain-pools/%i/domains" u1.Tenant.Value.DomainPoolId)
                        data

                ensureForbidden result
            }
