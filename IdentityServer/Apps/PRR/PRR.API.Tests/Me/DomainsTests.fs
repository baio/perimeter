namespace PRR.API.Tests.Me

open Akkling
open Common.Test.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FsUnit
open PRR.API.Tests.Utils
open PRR.Domain.Auth.SignUp
open PRR.Domain.Tenant.Domains
open PRR.Domain.Tenant.UserDomains
open PRR.System.Models
open Xunit
open Xunit.Abstractions
open Xunit.Priority

module Domains =
    let userData: Data =
        { FirstName = "First"
          LastName = "Last"
          Email = "user@user.com"
          Password = "#6VvR&^"
          QueryString = null }

    let mutable userToken: string = null

    let mutable testContext: UserTestContext option = None

    let mutable domainId: int option = None



    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``me-domains-api``(testFixture: TestFixture, output: ITestOutputHelper) =
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
        member __.``A Get user domains``() =
            task {
                let! result = testFixture.HttpGetAsync userToken "/me/management/domains"
                do! ensureSuccessAsync result
                let! result = readAsJsonAsync<TenantDomain array> result
                result |> should haveLength 2
                printf "+++ %A" result
            }
