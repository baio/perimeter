namespace PRR.API.Tests

open Akkling
open Common.Test.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FsUnit
open Microsoft.Extensions.DependencyInjection
open PRR.API.Tests.Utils
open PRR.Domain.Auth
open PRR.Domain.Auth.SignUp
open PRR.System.Models
open System
open Xunit
open Xunit.Abstractions
open Xunit.Priority

module SignUp =

    let mutable userToken: string = null
    let mutable actualEmail: SendMailParams option = None

    let waitHandle = new System.Threading.AutoResetEvent(false)

    let userData: Data =
        { FirstName = "First"
          LastName = "Last"
          Email = "user@user.com"
          Password = "#6VvR&^"
          QueryString = null }

    let sendMail: SendMail =
        fun data ->
            task {
                actualEmail <- Some data
                match data.Template with
                | ConfirmSignUpMail x ->
                    userToken <- x.Token
                waitHandle.Set() |> ignore
            }

    let mutable tenant: CreatedTenantInfo option = None
    let tenantWaitHandle = new System.Threading.AutoResetEvent(false)

    let mutable sysException: Exception = null

    let systemEventHandled =
        function
        | UserTenantCreatedEvent data ->
            tenant <- Some data
            tenantWaitHandle.Set() |> ignore
        | CommandFailureEvent e ->
            sysException <- e
            tenantWaitHandle.Set() |> ignore
        | QueryFailureEvent e ->
            sysException <- e
            tenantWaitHandle.Set() |> ignore
        | _ ->
            ()


    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``sign-up-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.``0 BeforeAll``() =
            testFixture.OverrideServices(fun services ->
                let sp = services.BuildServiceProvider()
                let systemEnv = sp.GetService<SystemEnv>()

                let systemEnv =
                    { systemEnv with
                          SendMail = sendMail
                          EventHandledCallback = systemEventHandled }

                let sys = PRR.System.Setup.setUp systemEnv
                services.AddSingleton<ICQRSSystem>(fun _ -> sys) |> ignore)

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

                let! result = testFixture.HttpPostAsync' "/api/auth/sign-up" invalidUserData

                ensureBadRequest result
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``B SignUp success``() =

            task {

                let! result = testFixture.HttpPostAsync' "/api/auth/sign-up" userData

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
                                    Password = x.Password
                                    ExpiredAt = x.ExpiredAt
                                    QueryString = None } }



                actualEmail |> should equal expectedEmail
            }


        [<Fact>]
        [<Priority(2)>]
        member __.``C SignUp confirm``() =

            task {

                let confirmData: SignUpConfirm.Models.Data =
                    { Token = userToken }

                let! result = testFixture.HttpPostAsync' "/api/auth/sign-up/confirm" confirmData

                do! ensureSuccessAsync result
            }

        [<Fact>]
        [<Priority(3)>]
        member __.``D SignUp confirm with the same token must fail``() =

            task {

                let confirmData: SignUpConfirm.Models.Data =
                    { Token = userToken }

                let! result = testFixture.HttpPostAsync' "/api/auth/sign-up/confirm" confirmData

                ensureUnauthorized result
            }

        [<Fact>]
        [<Priority(3)>]
        member __.``E SignUp with same email must fail``() =

            task {

                let! result = testFixture.HttpPostAsync' "/api/auth/sign-up" userData

                ensureConflict result }

        [<Fact>]
        [<Priority(3)>]
        member __.``F SignUp with different email should be success``() =

            task {

                let! _ = testFixture.HttpPostAsync' "/api/auth/sign-up" { userData with Email = "user2@user.com" }

                waitHandle.WaitOne() |> ignore

                let confirmData: SignUpConfirm.Models.Data =
                    { Token = userToken }

                let! result = testFixture.HttpPostAsync' "/api/auth/sign-up/confirm" confirmData

                do! ensureSuccessAsync result

                tenantWaitHandle.WaitOne(500) |> ignore

                sysException |> should be null
                tenant |> should be (not' null)
            }
