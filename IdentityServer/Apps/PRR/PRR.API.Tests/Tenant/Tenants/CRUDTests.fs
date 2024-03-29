﻿namespace PRR.API.Tests.Tenant.Tenants

open DataAvail.Test.Common
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.API.Tests.Utils
open PRR.Domain.Auth.SignUp
open PRR.Domain.Tenant
open PRR.Domain.Tenant.Tenants
open Xunit
open Xunit.Abstractions
open Xunit.Priority

module CRUD =

    let userData: Data =
        { FirstName = "First"
          LastName = "Last"
          Email = "user@user.com"
          Password = "#6VvR&^"
          ReturnUrl = null }

    let userPassword = "123"

    let mutable userToken: string = null

    let mutable testContext: UserTestContext option = None

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
    type ``tenant-crud-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.``0 BEFORE ALL``() =
            task {
                testContext <- Some(createUserTestContext testFixture)

                let! userToken' = createUser' false testContext.Value userData
                userToken <- userToken'
                printf "%s" userToken
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``A Create tenant with sandbox must be success``() =
            task {
                let sandboxData =
                    { DomainName = "domain"
                      EnvName = "env"
                      ApiName = "api"
                      AppName = "app"
                      Permissions = [| "read"; "write" |] }

                let data =
                    {| Name = "tenant-a"
                       Sandbox = sandboxData |}

                let! result = testFixture.Server2.HttpPostAsync userToken "/api/tenant/tenants" data
                do! ensureSuccessAsync result
                let! result = readAsJsonAsync<int> result
                ()
            }
