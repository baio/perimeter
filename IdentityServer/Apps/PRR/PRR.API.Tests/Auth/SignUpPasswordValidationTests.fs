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

module SignUpValidatePassword =

    let mutable userToken: string = null
    let mutable actualEmail: SendMailParams option = None

    let waitHandle = new System.Threading.AutoResetEvent(false)

    let userData: Data =
        { FirstName = "First"
          LastName = "Last"
          Email = "user@user.com"
          Password = "#6VvR&^" }

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
    type ``sign-up-validate-password-api``(testFixture: TestFixture, output: ITestOutputHelper) =
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
        member __.``A SignUp password min length``() =

            task {
                let invalidUserData: Data =
                    { FirstName = "First"
                      LastName = "Last"
                      Email = "max@gmail.com"
                      Password = "Aa!1" }

                let! result = testFixture.HttpPostAsync' "/auth/sign-up" invalidUserData

                ensureBadRequest result

                let! json = result.Content.ReadAsStringAsync()

                json |> should equal """{"password":["MIN_LENGTH:6"]}"""
            }

        [<Fact>]
        member __.``B SignUp password empty``() =

            task {
                let invalidUserData: Data =
                    { FirstName = "First"
                      LastName = "Last"
                      Email = "max@gmail.com"
                      Password = "" }

                let! result = testFixture.HttpPostAsync' "/auth/sign-up" invalidUserData

                ensureBadRequest result

                let! json = result.Content.ReadAsStringAsync()

                json |> should equal """{"password":["EMPTY_STRING"]}"""
            }

        [<Fact>]
        member __.``C SignUp password doesn't contain special characters``() =

            task {
                let invalidUserData: Data =
                    { FirstName = "First"
                      LastName = "Last"
                      Email = "max@gmail.com"
                      Password = "123456" }

                let! result = testFixture.HttpPostAsync' "/auth/sign-up" invalidUserData

                ensureBadRequest result

                let! json = result.Content.ReadAsStringAsync()

                json |> should equal """{"password":["MISS_UPPER_LETTER","MISS_LOWER_LETTER","MISS_SPECIAL_CHAR"]}"""
            }

        [<Fact>]
        member __.``D SignUp password doesn't contain number``() =

            task {
                let invalidUserData: Data =
                    { FirstName = "First"
                      LastName = "Last"
                      Email = "max@gmail.com"
                      Password = "Azz!@gyut&" }

                let! result = testFixture.HttpPostAsync' "/auth/sign-up" invalidUserData

                ensureBadRequest result

                let! json = result.Content.ReadAsStringAsync()

                json |> should equal """{"password":["MISS_DIGIT"]}"""
            }
