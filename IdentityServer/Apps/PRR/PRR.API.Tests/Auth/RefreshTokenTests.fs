namespace PRR.API.Tests

open DataAvail.Test.Common
open FSharp.Control.Tasks.V2.ContextInsensitive
open FsUnit
open PRR.API.Tests.Utils
open PRR.Domain.Auth
open PRR.Domain.Auth.SignUp
open PRR.Domain.Auth.LogIn.RefreshToken
open PRR.Domain.Tenant
open Xunit
open Xunit.Abstractions
open Xunit.Priority

module RefreshToken =

    [<CLIMutable>]
    type SignInResult =
        { id_token: string
          access_token: string
          refresh_token: string }

    let mutable accessToken: string = null
    let mutable refreshToken: string = null

    let mutable accessToken2: string = null
    let mutable refreshToken2: string = null
    let mutable confirmToken: string = null

    let confirmTokenWaitHandle =
        new System.Threading.AutoResetEvent(false)

    let mutable tenant: CreatedTenantInfo option = None

    let ownerData: PRR.Domain.Auth.SignUp.Models.Data =
        { FirstName = "First"
          LastName = "Last"
          Email = "user@user.com"
          Password = "#6VvR&^"
          ReturnUrl = null }


    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``refresh-token-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.``0 BeforeAll``() =
            task {

                let testContext = createUserTestContext testFixture

                let! result = createUser'' true testContext ownerData
                accessToken <- result.access_token
                refreshToken <- result.refresh_token
                return ()
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``A Wrong refresh token must be fail``() =

            task {

                let data: Data =
                    { Grant_Type = "refresh_token"
                      Refresh_Token = "xxxx" }

                let! result = testFixture.Server1.HttpPostAsync accessToken "/api/auth/token" data

                ensureUnauthorized result
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``B Correct refresh token with wrong access token must fail``() =

            task {

                let data: Data =
                    { Grant_Type = "refresh_token"
                      Refresh_Token = refreshToken }

                let! result = testFixture.Server1.HttpPostAsync "xxx" "/api/auth/token" data

                ensureUnauthorized result
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``C Refresh token must be success``() =

            task {

                let data: Data =
                    { Grant_Type = "refresh_token"
                      Refresh_Token = refreshToken }

                // Wait in order to get updated access token
                do! System.Threading.Tasks.Task.Delay(1000)

                let! result = testFixture.Server1.HttpPostAsync accessToken "/api/auth/token" data

                do! ensureSuccessAsync result

                let! result = readAsJsonAsync<SignInResult> result

                result.access_token |> should be (not' Null)

                result.id_token |> should be (not' Null)
                result.refresh_token |> should be (not' Null)

                result.access_token
                |> should not' (equal accessToken)

                result.refresh_token
                |> should not' (equal refreshToken)

                accessToken2 <- result.access_token

                refreshToken2 <- result.refresh_token
            }

        [<Fact>]
        [<Priority(2)>]
        member __.``D Refresh same token second time must fail``() =

            task {

                let data: Data =
                    { Grant_Type = "refresh_token"
                      Refresh_Token = refreshToken }

                let! result = testFixture.Server1.HttpPostAsync accessToken "/api/auth/token" data

                ensureUnauthorized result
            }

        [<Fact>]
        [<Priority(2)>]
        member __.``E Refresh with new token must be success``() =

            task {

                let data: Data =
                    { Grant_Type = "refresh_token"
                      Refresh_Token = refreshToken2 }

                // Wait in order to get updated access token
                do! System.Threading.Tasks.Task.Delay(1000)

                let! result = testFixture.Server1.HttpPostAsync accessToken2 "/api/auth/token" data

                do! ensureSuccessAsync result

                let! result = readAsJsonAsync<SignInResult> result

                result.access_token |> should be (not' Null)

                result.id_token |> should be (not' Null)
                result.refresh_token |> should be (not' Null)

                result.access_token
                |> should not' (equal accessToken)

                result.refresh_token
                |> should not' (equal refreshToken)

                result.access_token
                |> should not' (equal accessToken2)

                result.refresh_token
                |> should not' (equal refreshToken2)

                accessToken2 <- result.access_token

                refreshToken2 <- result.refresh_token
            }


        // TODO : Restore
        [<Fact>]
        [<Priority(3)>]
        member __.``F After logout user could not refresh token``() =

            task {

                let! logoutResult =
                    testFixture.Server1.HttpGetAsync'
                        (sprintf "/api/auth/logout?post_logout_redirect_uri=%s&id_token_hint=%s" "http://localhost:4200" accessToken2)

                do! ensureRedirectSuccessAsync logoutResult

                let data: Data =
                    { Grant_Type = "refresh_token"
                      Refresh_Token = refreshToken2 }

                do! System.Threading.Tasks.Task.Delay(100)

                let! result = testFixture.Server1.HttpPostAsync accessToken2 "/api/auth/token" data

                ensureUnauthorized result
            }
