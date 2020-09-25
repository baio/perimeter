namespace PRR.API.Tests.Tenant.Domains

open Akkling
open Common.Test.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FsUnit
open PRR.API.Tests.Utils
open PRR.Data.Entities
open PRR.Domain.Auth.SignUp
open PRR.Domain.Tenant.Domains
open PRR.Domain.Tenant.UserDomains
open PRR.System.Models
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

    let mutable domainId: int option = None

    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``domain-crud-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.``0 BEFORE ALL``() =
            task {
                testContext <- Some(createUserTestContext testFixture)
                let! userToken' = createUser' false testContext.Value userData
                userToken <- userToken'
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``A Create domain must be success``() =
            let domainPoolId =
                testContext.Value.GetTenant().DomainPoolId

            task {
                let data: PostLike = { EnvName = "stage" }

                let! result =
                    testFixture.HttpPostAsync
                        userToken
                        (sprintf "/api/tenant/domain-pools/%i/domains" domainPoolId)
                        data

                do! ensureSuccessAsync result

                let! result = readAsJsonAsync<int> result

                domainId <- Some(result)
            }

        [<Fact>]
        [<Priority(2)>]
        member __.``A.0 Login under newly created tenant``() =

            task {

                let! result = testFixture.HttpGetAsync userToken "/api/me/management/domains"

                do! ensureSuccessAsync result

                let! result = readAsJsonAsync<TenantDomain array> result

                let domain =
                    result
                    |> Seq.find (fun f -> f.Id = domainId.Value)

                let! res = logInUser testFixture domain.ManagementClientId userData.Email userData.Password

                userToken <- res.access_token

                ()
            }


        [<Fact>]
        [<Priority(3)>]
        member __.``A.1 Get domain must be success``() =
            let domainPoolId =
                testContext.Value.GetTenant().DomainPoolId

            task {

                let! result =
                    testFixture.HttpGetAsync
                        userToken
                        (sprintf "/api/tenant/domain-pools/%i/domains/%i" domainPoolId domainId.Value)

                do! ensureSuccessAsync result

                let! result = readAsJsonAsync<GetLike> result

                result |> should be (not' null)

                result.Id |> should equal domainId.Value

                result.EnvName |> should equal "stage"

                result.SigningAlgorithm
                |> should equal SigningAlgorithmType.RS256

                result.AccessTokenExpiresIn |> should equal 30

                result.DateCreated |> should be (not' null)
            }


        [<Fact>]
        [<Priority(4)>]
        member __.``B Update domain pool must be success``() =
            let domainPoolId =
                testContext.Value.GetTenant().DomainPoolId

            task {
                let data: PutLike =
                    { EnvName = "stage!"
                      SigningAlgorithm = "HS256"
                      AccessTokenExpiresIn = 500 }

                let! result =
                    testFixture.HttpPutAsync
                        userToken
                        (sprintf "/api/tenant/domain-pools/%i/domains/%i" domainPoolId domainId.Value)
                        data

                do! ensureSuccessAsync result
            }

        [<Fact>]
        [<Priority(5)>]
        member __.``C Get domain must be success``() =
            let domainPoolId =
                testContext.Value.GetTenant().DomainPoolId

            task {

                let! result =
                    testFixture.HttpGetAsync
                        userToken
                        (sprintf "/api/tenant/domain-pools/%i/domains/%i" domainPoolId domainId.Value)

                do! ensureSuccessAsync result

                let! result = readAsJsonAsync<GetLike> result

                result |> should be (not' null)

                result.Id |> should equal domainId.Value

                result.EnvName |> should equal "stage!"

                result.SigningAlgorithm
                |> should equal SigningAlgorithmType.HS256

                result.AccessTokenExpiresIn |> should equal 500

                result.DateCreated |> should be (not' null)
            }

        [<Fact>]
        [<Priority(6)>]
        member __.``D Update domain pool must be success``() =
            let domainPoolId =
                testContext.Value.GetTenant().DomainPoolId

            task {
                let data: PutLike =
                    { EnvName = "stage"
                      SigningAlgorithm = "RS256"
                      AccessTokenExpiresIn = 700 }

                let! result =
                    testFixture.HttpPutAsync
                        userToken
                        (sprintf "/api/tenant/domain-pools/%i/domains/%i" domainPoolId domainId.Value)
                        data

                do! ensureSuccessAsync result
            }

        [<Fact>]
        [<Priority(7)>]
        member __.``E Get domain must be success``() =
            let domainPoolId =
                testContext.Value.GetTenant().DomainPoolId

            task {

                let! result =
                    testFixture.HttpGetAsync
                        userToken
                        (sprintf "/api/tenant/domain-pools/%i/domains/%i" domainPoolId domainId.Value)

                do! ensureSuccessAsync result

                let! result = readAsJsonAsync<GetLike> result

                result |> should be (not' null)

                result.Id |> should equal domainId.Value

                result.EnvName |> should equal "stage"

                result.SigningAlgorithm
                |> should equal SigningAlgorithmType.RS256

                result.AccessTokenExpiresIn |> should equal 700

                result.DateCreated |> should be (not' null)
            }

        [<Fact>]
        [<Priority(8)>]
        member __.``F Delete domain must be success``() =
            let domainPoolId =
                testContext.Value.GetTenant().DomainPoolId

            task {
                let! result =
                    testFixture.HttpGetAsync
                        userToken
                        (sprintf "/api/tenant/domain-pools/%i/domains/%i" domainPoolId domainId.Value)

                do! ensureSuccessAsync result
            }
