namespace PRR.API.Tests

open DataAvail.Test.Common
open FSharp.Control.Tasks.V2.ContextInsensitive
open FsUnit
open PRR.API.Auth.Infra
open PRR.API.Tests.Utils
open PRR.Domain.Auth.LogInToken
open PRR.Domain.Auth.SignUp
open System
open System.Security.Cryptography
open Xunit
open Xunit.Abstractions
open Xunit.Priority

module LogInClientSecret =

    let signUpData: Data =
        { FirstName = "First"
          LastName = "Last"
          Email = "user@user.com"
          Password = "#6VvR&^"
          ReturnUrl = null }

    let mutable authCode: string = null

    let random = Random()

    let randomString length =

        let chars =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._~"

        [ 0 .. length ]
        |> Seq.map (fun x -> chars.[random.Next(chars.Length)])
        |> System.String.Concat


    let codeVerfier = randomString 128

    let sha256 = SHA256.Create()

    let codeChellenge =
        SHA256.getSha256Base64Hash sha256 codeVerfier
        |> LogInToken.cleanupCodeChallenge

    let mutable testContext: UserTestContext option = None

    let mutable permissionId: int option = None

    let redirectUri = "http://localhost:4200"


    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``login-client-secret-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.``0 BeforeAll``() =
            task {
                testContext <- Some(createUserTestContext testFixture)

                let! _ = createUser testContext.Value signUpData
                ()
            }


        [<Fact>]
        [<Priority(2)>]
        member __.``B Login with wrong ClientSecret should give error``() =

            let clientId =
                testContext
                    .Value
                    .GetTenant()
                    .TenantManagementApplicationClientId

            let logInData: PRR.Domain.Auth.LogIn.Models.Data =
                { Client_Id = clientId
                  Response_Type = "code"
                  State = "state"
                  Redirect_Uri = redirectUri
                  Scope = "openid profile email"
                  Email = signUpData.Email
                  Password = signUpData.Password
                  Code_Challenge = codeChellenge
                  Code_Challenge_Method = "S256" }


            task {
                let! authCode = logIn testFixture logInData

                let loginTokenData: PRR.Domain.Auth.LogInToken.Models.Data =
                    { Grant_Type = "authorization_code"
                      Code = authCode
                      Redirect_Uri = logInData.Redirect_Uri
                      Client_Id = clientId
                      Code_Verifier = sprintf "%s1" codeVerfier
                      Client_Secret = null }

                let! result = testFixture.Server1.HttpPostAsync' "/api/auth/token" loginTokenData
                do ensureUnauthorized result
            }


        [<Fact>]
        [<Priority(3)>]
        member __.``C Login with correct data should success``() =

            let clientId =
                testContext
                    .Value
                    .GetTenant()
                    .TenantManagementApplicationClientId

            let logInData: PRR.Domain.Auth.LogIn.Models.Data =
                { Client_Id = clientId
                  Response_Type = "code"
                  State = "state"
                  Redirect_Uri = redirectUri
                  Scope = "openid profile email"
                  Email = signUpData.Email
                  Password = signUpData.Password
                  Code_Challenge = codeChellenge
                  Code_Challenge_Method = "S256" }

            task {
                let! result = logIn testFixture logInData
                authCode <- result

                let loginTokenData: PRR.Domain.Auth.LogInToken.Models.Data =
                    { Grant_Type = "authorization_code"
                      Code = authCode
                      Redirect_Uri = logInData.Redirect_Uri
                      Client_Id = clientId
                      Code_Verifier = codeVerfier
                      Client_Secret = null }

                let! result' = testFixture.Server1.HttpPostAsync' "/api/auth/token" loginTokenData
                do! ensureSuccessAsync result'

                let! result =
                    result'
                    |> readAsJsonAsync<PRR.Domain.Auth.LogInToken.Models.Result>

                result.access_token |> should be (not' Empty)
                result.id_token |> should be (not' Empty)
                result.refresh_token |> should be (not' Empty)
            }


        [<Fact>]
        [<Priority(4)>]
        member __.``D Login token with the same code should give 401``() =
            let loginTokenData: PRR.Domain.Auth.LogInToken.Models.Data =
                { Grant_Type = "authorization_code"
                  Code = authCode
                  Redirect_Uri = redirectUri
                  Client_Id =
                      testContext
                          .Value
                          .GetTenant()
                          .TenantManagementApplicationClientId
                  Code_Verifier = codeVerfier
                  Client_Secret = null }

            task {
                let! result = testFixture.Server1.HttpPostAsync' "/api/auth/token" loginTokenData
                do ensureUnauthorized result
            }
