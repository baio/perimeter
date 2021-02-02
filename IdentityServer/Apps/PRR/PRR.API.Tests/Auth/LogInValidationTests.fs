namespace PRR.API.Tests

open DataAvail.Test.Common
open FSharp.Control.Tasks.V2.ContextInsensitive
open FsUnit
open PRR.API.Tests.Utils
open PRR.Domain.Auth.SignUp
open Xunit
open Xunit.Abstractions
open Xunit.Priority

module LogInValidation =

    let signUpData: Data =
        { FirstName = "First"
          LastName = "Last"
          Email = "user@user.com"
          Password = "#6VvR&^"
          QueryString = null }

    let logInData =
        {| client_id = "123"
           response_type = "code"
           state = "state"
           redirect_uri = "http://localhost:4200"
           scope = "openid profile email"
           email = signUpData.Email
           password = signUpData.Password
           code_challenge = "123"
           code_challenge_method = "S256" |}

    let mutable userToken: string = null

    let mutable testContext: UserTestContext option = None

    let mutable permissionId: int option = None


    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``login-validation-api``(testFixture: TestFixture, output: ITestOutputHelper) =
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
        member __.``A Login data with empty client_id should give validation error``() =
            task {
                let! result =
                    testFixture.Server1.HttpPostFormJsonAsync
                        userToken
                        "/api/auth/authorize"
                        {| logInData with client_id = "" |}

                do! ensureRedirectErrorAsync result
            }

        [<Fact>]
        member __.``B All empty fields should give validation error``() =
            task {
                let! result = testFixture.Server1.HttpPostFormJsonAsync userToken "/api/auth/authorize" {|  |}
                do! ensureRedirectErrorAsync result
            }

        [<Fact>]
        member __.``C response_type different from 'code' should give error``() =
            task {
                let! result =
                    testFixture.Server1.HttpPostFormJsonAsync
                        userToken
                        "/api/auth/authorize"
                        {| logInData with
                               response_type = "not_code" |}

                do! ensureRedirectErrorAsync result
            }

        [<Fact>]
        member __.``D redirect_uri not url``() =
            task {
                let! result =
                    testFixture.Server1.HttpPostFormJsonAsync
                        userToken
                        "/api/auth/authorize"
                        {| logInData with
                               redirect_uri = "redirect_uri" |}

                do! ensureRedirectErrorAsync result
            }

        [<Fact>]
        member __.``E scopes doesn't contain required scopes``() =
            task {
                let! result =
                    testFixture.Server1.HttpPostFormJsonAsync
                        userToken
                        "/api/auth/authorize"
                        {| logInData with scope = "openid" |}

                do! ensureRedirectErrorAsync result
            }

        [<Fact>]
        member __.``F email is not email should return error``() =
            task {
                let! result =
                    testFixture.Server1.HttpPostFormJsonAsync
                        userToken
                        "/api/auth/authorize"
                        {| logInData with email = "not_email" |}

                do! ensureRedirectErrorAsync result
            }

        [<Fact>]
        member __.``G code_challenge_method is not S256 should give error``() =
            task {
                let! result =
                    testFixture.Server1.HttpPostFormJsonAsync
                        userToken
                        "/api/auth/authorize"
                        {| logInData with
                               code_challenge_method = "not_S256" |}

                do! ensureRedirectErrorAsync result
            }
