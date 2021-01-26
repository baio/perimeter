namespace PRR.API.Tests.Tenant.Permissions

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
open TaskUtils
open Xunit
open Xunit.Abstractions
open Xunit.Priority

module NotFound =

    let user1Data: Data =
        { FirstName = "First"
          LastName = "XXX"
          Email = "user1@user.com"
          Password = "#6VvR&^"
          QueryString = null }

    let users =
        System.Collections.Generic.List<_>
            [ {| Data = user1Data
                 Token = None
                 Tenant = None |} ]

    let mutable testContext: UserTestContext option = None

    [<CLIMutable>]
    type GetLikeDto =
        { id: int
          name: string
          description: string
          dateCreated: System.DateTime }

    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``permissions-not-found-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.BeforeAll() =
            task {
                let testPermission: PostLike =
                    { Name = "test:permissions"
                      Description = "test description"
                      IsDefault = false }

                testContext <- Some(createUserTestContext testFixture)
                // create user 1 + tenant + permission
                let u = users.[0]

                let! userToken = createUser testContext.Value u.Data
                let tenant = testContext.Value.GetTenant()

                users.[0] <- {| u with
                                    Token = Some(userToken)
                                    Tenant = Some(tenant) |}
            }

        [<Fact>]
        member __.``POST Forbidden for unexistent tenant``() =
            let u1 = users.[0]
            task {
                let data: PostLike =
                    { Name = "test:permissions"
                      Description = "test description"
                      IsDefault = false }

                let! result = testFixture.HttpPostAsync u1.Token.Value (sprintf "/api/tenant/apis/100/permissions") data
                ensureNotFound result
            }

        [<Fact>]
        member __.``PUT NotFound for unexistent permissions``() =
            let u1 = users.[0]
            task {
                let data: PostLike =
                    { Name = "test:permissions"
                      Description = "test description"
                      IsDefault = false }

                let! result =
                    testFixture.HttpPutAsync
                        u1.Token.Value
                        (sprintf "/api/tenant/apis/%i/permissions/%i" u1.Tenant.Value.SampleApiId 100)
                        data

                ensureNotFound result
            }

        [<Fact>]
        member __.``GET NotFound for unexistent permissions``() =
            let u1 = users.[0]
            task {
                let! result =
                    testFixture.HttpGetAsync
                        u1.Token.Value
                        (sprintf "/api/tenant/apis/%i/permissions/%i" u1.Tenant.Value.SampleApiId 100)

                ensureNotFound result
            }


        [<Fact>]
        member __.``DELETE NotFound for unexistent permissions``() =
            let u1 = users.[0]
            task {
                let! result =
                    testFixture.HttpDeleteAsync
                        u1.Token.Value
                        (sprintf "/api/tenant/apis/%i/permissions/%i" u1.Tenant.Value.SampleApiId 100)

                ensureNotFound result
            }
