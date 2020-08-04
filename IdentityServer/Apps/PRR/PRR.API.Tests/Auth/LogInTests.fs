namespace PRR.API.Tests

open Common.Test.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FsUnit
open Microsoft.VisualBasic
open PRR.API.ErrorHandler
open PRR.API.Infra
open PRR.API.Tests.Utils
open PRR.Domain.Auth.SignUp
open System
open System.Security.Cryptography
open Xunit
open Xunit.Abstractions
open Xunit.Priority

module LogIn =

    let signUpData: Data =
        { FirstName = "First"
          LastName = "Last"
          Email = "user@user.com"
          Password = "#6VvR&^" }

    let mutable authCode: string = null

    let random = Random()

    let randomString length =

        let chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._~"
        [ 0 .. length ]
        |> Seq.map (fun x -> chars.[random.Next(chars.Length)])
        |> System.String.Concat


    let codeVerfier = randomString 128

    let sha256 = SHA256.Create()
    let codeChellenge = HashProvider.getSha256Hash' sha256 codeVerfier

    let logInData =
        {| client_id = "123"
           response_type = "code"
           state = "state"
           redirect_uri = "http://localhost:4200"
           scopes = [| "open_id"; "profile"; "email" |]
           email = signUpData.Email
           password = signUpData.Password
           code_challenge = codeChellenge
           code_challenge_method = "S256" |}

    let mutable userToken: string = null

    let mutable testContext: UserTestContext option = None

    let mutable permissionId: int option = None

    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``login-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.``0 BeforeAll``() =
            task {

                testContext <- Some(createUserTestContext testFixture)
                let! userToken' = createUser testContext.Value signUpData
                userToken <- userToken'
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``A Login with correct data should success``() =
            task {
                let! result' = testFixture.HttpPostAsync userToken "/auth/login" logInData
                do! ensureSuccessAsync result'
                let! result = result' |> readAsJsonAsync<PRR.Domain.Auth.LogIn.LogIn.Result>
                result.State |> should equal logInData.state
                result.Code |> should be (not' Empty)
                authCode <- result.Code
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``B Login token with correct data should success``() =
            let loginTokenData: PRR.Domain.Auth.LogInToken.Models.Data =
                { GrantType = "code"
                  Code = authCode
                  RedirectUri = "http://localhost:4200"
                  ClientId = testContext.Value.GetTenant().TenantManagementApplicationClientId
                  CodeVerifier = codeVerfier }
            task {
                let! result' = testFixture.HttpPostAsync userToken "/auth/token" loginTokenData
                do! ensureSuccessAsync result'
                let! result = result' |> readAsJsonAsync<PRR.Domain.Auth.SignIn.Models.SignInResult>
                printf "%O" result
            }
            // TODO : Validate payload,  ClientId, Code Challenges
            
       
