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

module RefreshToken =

    [<CLIMutable>]
    type SignInResult =
        { idToken: string
          accessToken: string
          refreshToken: string }

    let mutable accessToken: string = null
    let mutable refreshToken: string = null

    let mutable accessToken2: string = null
    let mutable refreshToken2: string = null
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
    type ``refresh-token-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.BeforeAll() =
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

                let validUserData: SignIn.Models.SignInData =
                    { Email = ownerData.Email
                      Password = ownerPassword
                      ClientId = tenant.Value.SampleApplicationClientId }

                let! result = testFixture.HttpPostAsync' "/auth/sign-in" validUserData
                let! result = readAsJsonAsync<SignInResult> result
                accessToken <- result.accessToken
                refreshToken <- result.refreshToken

                return ()
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``Wrong refresh token must be fail``() =

            task {

                let data: RefreshToken.Models.Data =
                    { RefreshToken = "xxxx" }

                let! result = testFixture.HttpPostAsync accessToken "/auth/refresh-token" data

                ensureUnauthorized result
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``Correct refresh token with wrong access token must fail``() =

            task {

                let data: RefreshToken.Models.Data =
                    { RefreshToken = refreshToken }

                let! result = testFixture.HttpPostAsync "xxx" "/auth/refresh-token" data

                ensureUnauthorized result
            }        

        [<Fact>]
        [<Priority(1)>]
        member __.``Refresh token must be success``() =

            task {

                let data: RefreshToken.Models.Data =
                    { RefreshToken = refreshToken }

                // Wait in order to get updated access token
                do! System.Threading.Tasks.Task.Delay(1000)

                let! result = testFixture.HttpPostAsync accessToken "/auth/refresh-token" data

                do! ensureSuccessAsync result

                let! result = readAsJsonAsync<SignInResult> result

                result.accessToken |> should be (not' Null)
                result.idToken |> should be (not' Null)
                result.refreshToken |> should be (not' Null)

                result.accessToken |> should not' (equal accessToken)
                result.refreshToken |> should not' (equal refreshToken)

                accessToken2 <- result.accessToken
                refreshToken2 <- result.refreshToken
            }
            
        [<Fact>]
        [<Priority(2)>]
        member __.``Refresh same token second time must fail``() =

            task {

                let data: RefreshToken.Models.Data =
                    { RefreshToken = refreshToken }

                let! result = testFixture.HttpPostAsync accessToken "/auth/refresh-token" data

                ensureUnauthorized result
            }            

        [<Fact>]
        [<Priority(2)>]
        member __.``Refresh with new token must be success``() =

            task {

                let data: RefreshToken.Models.Data =
                    { RefreshToken = refreshToken2 }

                // Wait in order to get updated access token
                do! System.Threading.Tasks.Task.Delay(1000)

                let! result = testFixture.HttpPostAsync accessToken2 "/auth/refresh-token" data

                do! ensureSuccessAsync result

                let! result = readAsJsonAsync<SignInResult> result

                result.accessToken |> should be (not' Null)
                result.idToken |> should be (not' Null)
                result.refreshToken |> should be (not' Null)

                result.accessToken |> should not' (equal accessToken)
                result.refreshToken |> should not' (equal refreshToken)

                result.accessToken |> should not' (equal accessToken2)
                result.refreshToken |> should not' (equal refreshToken2)
            }
