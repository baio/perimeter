namespace PRR.API.Tests

open Akkling
open Common.Test.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FsUnit
open PRR.API.Tests.Utils
open PRR.Domain.Auth.SignUp
open PRR.Domain.Tenant.Permissions
open PRR.System.Models
open Xunit
open Xunit.Abstractions
open Xunit.Priority

module LogIn =

    let signUpData: Data =
        { FirstName = "First"
          LastName = "Last"
          Email = "user@user.com"
          Password = "#6VvR&^" }

    let logInData =
        {| client_id = ""
           response_type = "code"
           state = "state"
           redirect_uri = "http://localhost:4200"
           scopes = [| "open_id"; "profile"; "email" |]
           email = signUpData.Email
           password = signUpData.Password
           code_challenge = "123"
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
        member __.``A Login data with empty client_id should give validation error``() =
            task {
                let! result = testFixture.HttpPostAsync userToken "/auth/login" logInData
                do ensureBadRequest result                
            }
            
