namespace PRR.API.Tests

open Akka.Actor
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

module RefreshTokenPersistence =

    [<CLIMutable>]
    type SignInResult =
        { id_token: string
          access_token: string
          refresh_token: string }

    let mutable accessToken: string = null
    let mutable refreshToken: string = null

    let mutable accessToken2: string = null
    let mutable refreshToken2: string = null
    let mutable confirmToken: string = null

    let confirmTokenWaitHandle =
        new System.Threading.AutoResetEvent(false)

    let mutable tenant: CreatedTenantInfo option = None

    let tenantWaitHandle =
        new System.Threading.AutoResetEvent(false)

    let ownerData: Data =
        { FirstName = "First"
          LastName = "Last"
          Email = "user@user.com"
          Password = "#6VvR&^"
          QueryString = null }

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
        | _ -> ()

    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``refresh-token-persistence-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.BeforeAll() =

            testFixture.OverrideServices(fun services ->
                let sp = services.BuildServiceProvider()
                let systemEnv = sp.GetService<SystemEnv>()

                let systemEnv =
                    { systemEnv with
                          EventHandledCallback = systemEventHandled }

                let sys = PRR.System.Setup.setUp systemEnv
                services.AddSingleton<ICQRSSystem>(fun _ -> sys)
                |> ignore)

            task {

                let testContext = createUserTestContext testFixture

                let! result = createUser'' true testContext ownerData
                accessToken <- result.access_token
                refreshToken <- result.refresh_token
                return ()
                                
            }


        [<Fact>]
        [<Priority(1)>]
        member __.``Refresh token must be success``() =

            task {

                // Restart refresh token actor
                let sys =
                    testFixture.Server.Services.GetService<ICQRSSystem>()

                sys.CommandsRef
                <! RefreshTokenCommand(RefreshToken.Restart)

                let data: RefreshToken.Models.Data = { RefreshToken = refreshToken }

                // Wait in order to get updated access token
                do! System.Threading.Tasks.Task.Delay(1000)

                let! result = testFixture.HttpPostAsync accessToken "/auth/refresh-token" data

                do! ensureSuccessAsync result

                let! result = readAsJsonAsync<SignInResult> result

                result.access_token |> should be (not' Null)

                result.id_token |> should be (not' Null)
                result.refresh_token |> should be (not' Null)

                result.access_token
                |> should not' (equal accessToken)

                result.refresh_token
                |> should not' (equal refreshToken)

                accessToken2 <- result.access_token

                refreshToken2 <- result.refresh_token
            }


        [<Fact>]
        [<Priority(2)>]
        member __.``Refresh token snapshot``() =

            task {

                // Restart refresh token actor
                let sys =
                    testFixture.Server.Services.GetService<ICQRSSystem>()

                sys.CommandsRef
                <! RefreshTokenCommand(RefreshToken.MakeSnapshot)
                sys.CommandsRef
                <! RefreshTokenCommand(RefreshToken.Restart)

                Thread.Sleep(1000)
            }
