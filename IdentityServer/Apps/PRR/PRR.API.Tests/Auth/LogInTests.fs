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
    let codeChellenge = HashProvider.getSha256Hash sha256 codeVerfier

    let logInData: PRR.Domain.Auth.LogIn.Models.Data =
        { Client_Id = "123"
          Response_Type = "code"
          State = "state"
          Redirect_Uri = "http://localhost:4200"
          Scopes = [| "open_id"; "profile"; "email" |]
          Email = signUpData.Email
          Password = signUpData.Password
          Code_Challenge = codeChellenge
          Code_Challenge_Method = "S256" }

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
            let loginData =
                { logInData with Client_Id = testContext.Value.GetTenant().TenantManagementApplicationClientId }
            task {
                let! result' = testFixture.HttpPostAsync userToken "/auth/login" loginData
                do! ensureSuccessAsync result'
                let! result = result' |> readAsJsonAsync<PRR.Domain.Auth.LogIn.Models.Result>
                result.State |> should equal logInData.State
                result.Code |> should be (not' Empty)
                authCode <- result.Code
            }

        [<Fact>]
        [<Priority(2)>]
        member __.``B Login token with correct data should success``() =
            let loginTokenData: PRR.Domain.Auth.LogInToken.Models.Data =
                { Grant_Type = "code"
                  Code = authCode
                  Redirect_Uri = logInData.Redirect_Uri
                  Client_Id = testContext.Value.GetTenant().TenantManagementApplicationClientId
                  Code_Verifier = codeVerfier }
            task {
                let! result' = testFixture.HttpPostAsync userToken "/auth/token" loginTokenData
                do! ensureSuccessAsync result'
                let! result = result' |> readAsJsonAsync<PRR.Domain.Auth.SignIn.Models.SignInResult>
                printf "%O" result
            } 

        [<Fact>]
        [<Priority(3)>]
        member __.``C Login token with the same code should give 401``() =
            let loginTokenData: PRR.Domain.Auth.LogInToken.Models.Data =
                { Grant_Type = "code"
                  Code = authCode
                  Redirect_Uri = logInData.Redirect_Uri
                  Client_Id = testContext.Value.GetTenant().TenantManagementApplicationClientId
                  Code_Verifier = codeVerfier }
            task {
                let! result = testFixture.HttpPostAsync userToken "/auth/token" loginTokenData
                do ensureUnauthorized result
            } 
