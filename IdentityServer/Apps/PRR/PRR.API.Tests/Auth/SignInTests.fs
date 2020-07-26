namespace PRR.API.Tests

open Akkling
open Common.Test.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FsUnit
open Microsoft.Extensions.DependencyInjection
open NUnit.Framework.Internal
open PRR.API.Tests.Utils
open PRR.Domain.Auth
open PRR.Domain.Auth.SignUp
open PRR.System.Models
open System
open System.Threading
open Xunit
open Xunit.Abstractions
open Xunit.Priority

module SignIn =

    [<CLIMutable>]
    type SignInResult =
        { idToken: string
          accessToken: string
          refreshToken: string }

    let mutable confirmToken: string = null
    let confirmTokenWaitHandle = new System.Threading.AutoResetEvent(false)

    let mutable tenant: CreatedTenantInfo option = None
    let tenantWaitHandle = new System.Threading.AutoResetEvent(false)

    let ownerData: Data =
        { FirstName = "First"
          LastName = "Last"
          Email = "user@user.com" }

    let ownerPassword = "123"

    let systemEventHandled =
        function
        | UserSignedUpEvent data ->
            confirmToken <- data.Token
            confirmTokenWaitHandle.Set() |> ignore
        | UserTenantCreatedEvent data ->
            tenant <- Some data
            tenantWaitHandle.Set() |> ignore
        | CommandFailureEvent _ ->
            confirmTokenWaitHandle.Set() |> ignore
            tenantWaitHandle.Set() |> ignore
        | QueryFailureEvent _ ->
            confirmTokenWaitHandle.Set() |> ignore
            tenantWaitHandle.Set() |> ignore
        | _ ->
            ()


    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``sign-in-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.``0 BeforeAll``() =
            testFixture.OverrideServices(fun services ->
                let sp = services.BuildServiceProvider()
                let systemEnv = sp.GetService<SystemEnv>()
                let systemEnv =
                    { systemEnv with EventHandledCallback = systemEventHandled }
                let sys = PRR.System.Setup.setUp systemEnv
                services.AddSingleton<ICQRSSystem>(fun _ -> sys) |> ignore)

            task {
                let! _ = testFixture.HttpPostAsync' "/auth/sign-up" ownerData
                confirmTokenWaitHandle.WaitOne() |> ignore
                let confirmData: SignUpConfirm.Models.Data =
                    { Token = confirmToken
                      Password = ownerPassword }
                let! _ = testFixture.HttpPostAsync' "/auth/sign-up/confirm" confirmData
                tenantWaitHandle.WaitOne() |> ignore
                return ()
            }


        [<Fact>]
        [<Priority(1)>]
        member __.``A Sign In must be success``() =

            tenant.IsSome |> should be True

            task {

                let data: SignIn.Models.SignInData =
                    { Email = ownerData.Email
                      Password = ownerPassword
                      ClientId = tenant.Value.SampleApplicationClientId }

                let! result = testFixture.HttpPostAsync' "/auth/sign-in" data

                do! ensureSuccessAsync result

                let! result = readAsJsonAsync<SignInResult> result

                result.accessToken |> should be (not' Null)
                result.idToken |> should be (not' Null)
                result.refreshToken |> should be (not' Null)
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``B Log In must be success``() =

            tenant.IsSome |> should be True

            task {

                let data: SignIn.Models.LogInData =
                    { Email = ownerData.Email
                      Password = ownerPassword }

                let! result = testFixture.HttpPostAsync' "/auth/log-in" data

                do! ensureSuccessAsync result

                let! result = readAsJsonAsync<SignInResult> result

                result.accessToken |> should be (not' Null)
                result.idToken |> should be (not' Null)
                result.refreshToken |> should be (not' Null)
            }
