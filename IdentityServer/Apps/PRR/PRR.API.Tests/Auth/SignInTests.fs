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
    type ``sign-in-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        // [<Fact>]
        [<Priority(-1)>]
        member __.``0 BeforeAll``() =

            task {
                let! _ = testFixture.HttpPostAsync' "/api/auth/sign-up" ownerData
                confirmTokenWaitHandle.WaitOne() |> ignore
                let confirmData: SignUpConfirm.Models.Data = { Token = confirmToken }
                let! _ = testFixture.HttpPostAsync' "/api/auth/sign-up/confirm" confirmData
                tenantWaitHandle.WaitOne() |> ignore
                return ()
            }


        // [<Fact>]
        [<Priority(1)>]
        member __.``A LogIn must be success``() =

            tenant.IsSome |> should be True

            task {

                let! result =
                    logInUser testFixture tenant.Value.SampleApplicationClientId ownerData.Email ownerData.Password

                result.access_token |> should be (not' Null)

                result.id_token |> should be (not' Null)
                result.refresh_token |> should be (not' Null)
            }
