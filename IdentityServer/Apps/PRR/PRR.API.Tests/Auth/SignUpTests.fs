namespace PRR.API.Tests

open DataAvail.Test.Common
open FSharp.Control.Tasks.V2.ContextInsensitive
open FsUnit
open Microsoft.Extensions.DependencyInjection
open PRR.API.Infra.Mail.Models
open PRR.API.Tests.Utils
open PRR.Domain.Auth
open PRR.Domain.Auth.Common
open PRR.Domain.Auth.SignUp
open System
open PRR.Domain.Tenant
open Xunit
open Xunit.Abstractions
open Xunit.Priority

module SignUp =

    let mutable userToken: string = null
    let mutable actualEmail: SendMailParams option = None

    let waitHandle =
        new System.Threading.AutoResetEvent(false)

    let userData: Data =
        { FirstName = "First"
          LastName = "Last"
          Email = "user@user.com"
          Password = "#6VvR&^"
          QueryString = null }

    let sendMail: SendMail =
        fun data ->
            actualEmail <- Some data

            match data.Template with
            | ConfirmSignUpMail x -> userToken <- x.Token

            task { waitHandle.Set() }

    let mutable tenant: CreatedTenantInfo option = None

    let tenantWaitHandle =
        new System.Threading.AutoResetEvent(false)

    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``sign-up-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.``0 BeforeAll``() =

            testFixture.Server1.OverrideServices(fun services ->
                services.AddSingleton<ISendMailProvider>(SendMailProvider sendMail)
                |> ignore)

        [<Fact>]
        [<Priority(1)>]
        member __.``A SignUp invalid data must fail``() =

            task {
                let invalidUserData: Data =
                    { FirstName = ""
                      LastName = ""
                      Email = "xxx"
                      Password = "#6VvR&^"
                      QueryString = null }

                let! result = testFixture.Server1.HttpPostAsync' "/api/auth/sign-up" invalidUserData

                ensureBadRequest result
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``B SignUp success``() =

            task {

                let! result = testFixture.Server1.HttpPostAsync' "/api/auth/sign-up" userData

                do! ensureSuccessAsync result

                waitHandle.WaitOne() |> ignore

                let (ConfirmSignUpMail x) = actualEmail.Value.Template

                // expected email
                let expectedEmail =
                    Some
                        { From = "admin"
                          To = userData.Email
                          Subject = "welcome"
                          Template =
                              ConfirmSignUpMail
                                  { FirstName = userData.FirstName
                                    LastName = userData.LastName
                                    Email = userData.Email
                                    Token = userToken
                                    QueryString = None } }



                actualEmail |> should equal expectedEmail
            }


        [<Fact>]
        [<Priority(2)>]
        member __.``C SignUp confirm``() =

            task {

                let confirmData: SignUpConfirm.Models.Data = { Token = userToken }

                let! result = testFixture.Server1.HttpPostAsync' "/api/auth/sign-up/confirm" confirmData

                do! ensureSuccessAsync result
            }

        [<Fact>]
        [<Priority(3)>]
        member __.``D SignUp confirm with the same token must fail``() =

            task {

                let confirmData: SignUpConfirm.Models.Data = { Token = userToken }

                let! result = testFixture.Server1.HttpPostAsync' "/api/auth/sign-up/confirm" confirmData

                ensureUnauthorized result
            }

        [<Fact>]
        [<Priority(3)>]
        member __.``E SignUp with same email must fail``() =

            task {

                let! result = testFixture.Server1.HttpPostAsync' "/api/auth/sign-up" userData

                ensureConflict result
            }

        [<Fact>]
        [<Priority(3)>]
        member __.``F SignUp with different email should be success``() =

            task {

                let! _ =
                    testFixture.Server1.HttpPostAsync'
                        "/api/auth/sign-up"
                        { userData with
                              Email = "user2@user.com" }

                waitHandle.WaitOne() |> ignore

                let confirmData: SignUpConfirm.Models.Data = { Token = userToken }

                let! result = testFixture.Server1.HttpPostAsync' "/api/auth/sign-up/confirm" confirmData

                do! ensureSuccessAsync result
            }
