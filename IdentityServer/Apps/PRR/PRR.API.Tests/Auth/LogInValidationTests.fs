namespace PRR.API.Tests

open Common.Test.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FsUnit
open PRR.API.ErrorHandler
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
                let! result = testFixture.HttpPostFormJsonAsync userToken "/auth/login" {| logInData with client_id = "" |}
                do! ensureRedirectErrorAsync result                
            }

        [<Fact>]
        member __.``B All empty fields should give validation error``() =
            task {
                let! result = testFixture.HttpPostFormJsonAsync userToken "/auth/login" {|  |}
                do! ensureRedirectErrorAsync result
                
                
                (*
                let! result' = readAsJsonAsync<ErrorDataDTO<Map<string, string array>>> result

                let expected =
                    [ ("client_id", [| "EMPTY_STRING" |])
                      ("code_challenge", [| "EMPTY_STRING" |])
                      ("code_challenge_method", [| "EMPTY_STRING" |])
                      ("email", [| "EMPTY_STRING" |])
                      ("password", [| "EMPTY_STRING" |])
                      ("redirect_uri", [| "EMPTY_STRING" |])
                      ("response_type", [| "EMPTY_STRING" |])
                      ("scope", [| "EMPTY_STRING" |]) ]
                    |> Map

                result'.Data |> should equal expected
                *)
            }

        [<Fact>]
        member __.``C response_type different from 'code' should give error``() =
            task {
                let! result = testFixture.HttpPostFormJsonAsync userToken "/auth/login"
                                  {| logInData with response_type = "not_code" |}
                                  
                do! ensureRedirectErrorAsync result
                (*
                let! result' = readAsJsonAsync<ErrorDataDTO<Map<string, string array>>> result

                let expected =
                    [ ("response_type", [| "NOT_CONTAINS_STRING:code" |]) ] |> Map

                printf "+++ %A" result'.Data

                result'.Data |> should equal expected
                *)
            }

        [<Fact>]
        member __.``D redirect_uri not url``() =
            task {
                let! result = testFixture.HttpPostFormJsonAsync userToken "/auth/login"
                                  {| logInData with redirect_uri = "redirect_uri" |}
                
                do! ensureRedirectErrorAsync result
                (*
                do ensureBadRequest result

                let! result' = readAsJsonAsync<ErrorDataDTO<Map<string, string array>>> result

                let expected =
                    [ ("redirect_uri", [| "NOT_URL_STRING" |]) ] |> Map

                printf "+++ %A" result'.Data

                result'.Data |> should equal expected
                *)
            }

        [<Fact>]
        member __.``E scopes doesn't contain required scopes``() =
            task {
                let! result = testFixture.HttpPostFormJsonAsync userToken "/auth/login" {| logInData with scope = "openid" |}
                do! ensureRedirectErrorAsync result
                (*
                do ensureBadRequest result

                let! result' = readAsJsonAsync<ErrorDataDTO<Map<string, string array>>> result

                let expected =
                    [ ("scope", [| "NOT_CONTAINS_ALL_STRING:openid,profile" |]) ] |> Map

                printf "+++ %A" result'.Data

                result'.Data |> should equal expected
                *)
            }

        [<Fact>]
        member __.``F email is not email should return error``() =
            task {
                let! result = testFixture.HttpPostFormJsonAsync userToken "/auth/login"
                                  {| logInData with email = "not_email" |}
                do! ensureRedirectErrorAsync result                  
                (*                                      
                do ensureBadRequest result

                let! result' = readAsJsonAsync<ErrorDataDTO<Map<string, string array>>> result

                let expected =
                    [ ("email", [| "NOT_EMAIL_STRING" |]) ] |> Map

                printf "+++ %A" result'.Data

                result'.Data |> should equal expected
                *)
            }

        [<Fact>]
        member __.``G code_challenge_method is not S256 should give error``() =
            task {
                let! result = testFixture.HttpPostFormJsonAsync userToken "/auth/login"
                                  {| logInData with code_challenge_method = "not_S256" |}
                do! ensureRedirectErrorAsync result                                  
                (*                                  
                do ensureBadRequest result

                let! result' = readAsJsonAsync<ErrorDataDTO<Map<string, string array>>> result

                let expected =
                    [ ("code_challenge_method", [| "NOT_CONTAINS_STRING:S256" |]) ] |> Map

                printf "+++ %A" result'.Data

                result'.Data |> should equal expected
                *)
            }
