﻿namespace PRR.API.Tests.Tenant.Applications

open DataAvail.Test.Common
open FSharp.Control.Tasks.V2.ContextInsensitive
open FsUnit
open PRR.API.Tests.Utils
open PRR.Data.Entities
open PRR.Domain.Auth.SignUp
open PRR.Domain.Tenant
open PRR.Domain.Tenant.Applications
open System
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

    let mutable applicationId: int option = None

    [<CLIMutable>]
    type PermissionGetLikeDto = { id: int; name: string }

    [<CLIMutable>]
    type GetLikeDto =
        { id: int
          name: string
          clientId: string
          dateCreated: DateTime
          idTokenExpiresIn: int
          refreshTokenExpiresIn: int }

    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``applications-crud-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.``0 Before All``() =
            task {
                testContext <- Some(createUserTestContext testFixture)

                let! userToken' = createUser testContext.Value userData
                userToken' |> should be (not' null)
                userToken <- userToken'
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``A Create application must be success``() =
            let domainId = testContext.Value.GetTenant().DomainId

            task {
                let data: PostLike =
                    { Name = "App 1"
                      GrantTypes =
                          [| GrantType.AuthorizationCodePKCE.ToString()
                             GrantType.RefreshToken.ToString() |] }

                let! result =
                    testFixture.Server2.HttpPostAsync
                        userToken
                        (sprintf "/api/tenant/domains/%i/applications" domainId)
                        data

                do! ensureSuccessAsync result

                let! result = readAsJsonAsync<int> result

                applicationId <- Some(result)
            }

        [<Fact>]
        [<Priority(2)>]
        member __.``B Get app must be success``() =
            let domainId = testContext.Value.GetTenant().DomainId

            task {
                let! result =
                    testFixture.Server2.HttpGetAsync
                        userToken
                        (sprintf "/api/tenant/domains/%i/applications/%i" domainId applicationId.Value)

                do! ensureSuccessAsync result

                let! result = readAsJsonAsync<GetLikeDto> result

                result |> should be (not' null)
                result.id |> should equal applicationId.Value

                result.name |> should equal "App 1"

                result.clientId |> should be (not' null)

                result.dateCreated |> should be (not' null)
                result.idTokenExpiresIn |> should be (not' null)

                result.refreshTokenExpiresIn
                |> should be (not' null)
            }

        [<Fact>]
        [<Priority(3)>]
        member __.``C Update app must be success``() =
            let domainId = testContext.Value.GetTenant().DomainId

            task {
                let data: PutLike =
                    { Name = "App 1 Updated"
                      IdTokenExpiresIn = 100
                      RefreshTokenExpiresIn = 100
                      AllowedCallbackUrls = "https://some.com https://some1.com"
                      AllowedLogoutCallbackUrls = "https://some.com"
                      SSOEnabled = true
                      GrantTypes =
                          [| GrantType.AuthorizationCodePKCE.ToString()
                             GrantType.RefreshToken.ToString() |] }

                let! result =
                    testFixture.Server2.HttpPutAsync
                        userToken
                        (sprintf "/api/tenant/domains/%i/applications/%i" domainId applicationId.Value)
                        data

                do! ensureSuccessAsync result
            }

        [<Fact>]
        [<Priority(4)>]
        member __.``D Get app after update must be success``() =
            let domainId = testContext.Value.GetTenant().DomainId

            task {
                let! result =
                    testFixture.Server2.HttpGetAsync
                        userToken
                        (sprintf "/api/tenant/domains/%i/applications/%i" domainId applicationId.Value)

                do! ensureSuccessAsync result

                let! result = readAsJsonAsync<GetLikeDto> result

                result |> should be (not' null)
                result.id |> should equal applicationId.Value
                result.name |> should equal "App 1 Updated"
                result.clientId |> should be (not' null)
                result.dateCreated |> should be (not' null)
                result.idTokenExpiresIn |> should be (not' null)

                result.refreshTokenExpiresIn
                |> should be (not' null)
            }

        [<Fact>]
        [<Priority(5)>]
        member __.``E Delete role must be success``() =
            let domainId = testContext.Value.GetTenant().DomainId

            task {
                let! result =
                    testFixture.Server2.HttpDeleteAsync
                        userToken
                        (sprintf "/api/tenant/domains/%i/applications/%i" domainId applicationId.Value)

                do! ensureSuccessAsync result
            }
