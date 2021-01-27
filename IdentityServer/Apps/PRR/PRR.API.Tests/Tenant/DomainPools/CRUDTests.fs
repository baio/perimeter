namespace PRR.API.Tests.Tenant.DomainPools

open Common.Test.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FsUnit
open PRR.API.Tests.Utils
open PRR.Domain.Auth.SignUp
open PRR.Domain.Tenant.DomainPools
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

    let userPassword = "123"

    let mutable userToken: string = null

    let mutable testContext: UserTestContext option = None

    let mutable tenantId: int option = None
    let mutable domainPoolId: int option = None

    [<CLIMutable>]
    type GetLikeDomainDto =
        { id: int
          name: string
          dateCreated: System.DateTime }

    [<CLIMutable>]
    type GetLikeDto =
        { id: int
          name: string
          dateCreated: System.DateTime
          domains: GetLikeDomainDto seq }

    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``domain-pool-crud-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.``0 BEFORE ALL``() =
            task {
                testContext <- Some(createUserTestContext testFixture)
                let! userToken' = createUser' false testContext.Value userData
                let tenant = testContext.Value.GetTenant()
                tenantId <- Some tenant.TenantId
                userToken <- userToken'
                printf "%s" userToken
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``A Create domain pool must be success``() =
            task {
                let data: PostLike =
                    { Name = "domain 1"
                      Identifier = "domain-1" }

                let! result =
                    testFixture.HttpPostAsync userToken (sprintf "/api/tenants/%i/domain-pools" tenantId.Value) data

                do! ensureSuccessAsync result

                let! result = readAsJsonAsync<int> result

                domainPoolId <- Some(result)
            }

        [<Fact>]
        [<Priority(2)>]
        member __.``B Update domain pool must be success``() =
            task {
                let data: PostLike =
                    { Name = "domain 1 updated"
                      Identifier = "domain-1-updated" }

                let! result =
                    testFixture.HttpPutAsync
                        userToken
                        (sprintf "/api/tenants/%i/domain-pools/%i" tenantId.Value domainPoolId.Value)
                        data

                do! ensureSuccessAsync result
            }

        [<Fact>]
        [<Priority(3)>]
        member __.``C.1 Get domain must be success``() =
            task {
                let expected: PostLike =
                    { Name = "domain 1 updated"
                      Identifier = "domain-1-updated" }

                let! result =
                    testFixture.HttpGetAsync
                        userToken
                        (sprintf "/api/tenants/%i/domain-pools/%i" tenantId.Value domainPoolId.Value)

                do! ensureSuccessAsync result

                let! result = readAsJsonAsync<GetLikeDto> result

                result |> should be (not' null)
                result.id |> should equal domainPoolId.Value
                result.name |> should equal expected.Name
                result.dateCreated |> should be (not' null)
            }

        [<Fact>]
        [<Priority(3)>]
        member __.``C.2 Get domains list be success``() =
            task {
                let! result = testFixture.HttpGetAsync userToken (sprintf "/api/tenants/%i/domain-pools" tenantId.Value)
                do! ensureSuccessAsync result
                let! result = readAsJsonAsync<ListResponse> result
                result |> should be (not' null)
                result.Pager |> should be (not' null)
                result.Items |> should be (not' null)
            }


        // TODO : Created domain pool must contains domain manager api / app and sample api / app
        // Tenant Owner must be the owner of the domain. Domain Pool Creator must be in Super-Admin role

        [<Fact>]
        [<Priority(4)>]
        member __.``D Delete domain must be success``() =
            task {
                let! result =
                    testFixture.HttpDeleteAsync
                        userToken
                        (sprintf "/api/tenants/%i/domain-pools/%i" tenantId.Value domainPoolId.Value)

                do! ensureSuccessAsync result
            }
