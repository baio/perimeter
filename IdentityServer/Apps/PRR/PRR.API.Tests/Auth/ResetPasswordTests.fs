namespace PRR.API.Tests.Auth

open Common.Test.Utils
open FSharp.Control.Tasks.V2.ContextInsensitive
open FsUnit
open NUnit.Framework.Internal
open PRR.API.Tests
open PRR.API.Tests.Utils
open PRR.Domain.Auth.ResetPasswordConfirm
open PRR.System.Models
open Xunit
open Xunit.Abstractions
open Xunit.Priority

module ResetPasswordTests =

    let user1Data: PRR.Domain.Auth.SignUp.Models.Data =
        { FirstName = "First"
          LastName = "XXX"
          Email = "user1@user.com"
          Password = "#6VvR&^"
          QueryString = null }

    let private users =
        System.Collections.Generic.List<_>
            [ {| Data = user1Data |} ]

    let mutable testContext: UserTestContext option = None

    let mutable accessToken: string option = None


    [<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
    type ``reset-password-api``(testFixture: TestFixture, output: ITestOutputHelper) =
        do setConsoleOutput output
        interface IClassFixture<TestFixture>

        [<Fact>]
        [<Priority(-1)>]
        member __.``0 BEFORE ALL``() =
            task {
                testContext <- Some(createUserTestContext testFixture)
                let! res = createUser' false testContext.Value users.[0].Data
                accessToken <- Some res
            }

        [<Fact>]
        [<Priority(1)>]
        member __.``A Reset password must work``() =
            task {
                let resetPasswordData: PRR.Domain.Auth.ResetPassword.Models.Data = { Email = users.[0].Data.Email }
                let! res = testFixture.HttpPostAsync' "/api/auth/reset-password" resetPasswordData
                do! ensureSuccessAsync res
                testContext.Value.ResetPasswordTokenHandle.WaitOne() |> ignore
                testContext.Value.GetResetPasswordToken() |> should be (not' null)
            }

        [<Fact>]
        [<Priority(2)>]
        member __.``B Reset password confirm must work``() =
            task {
                let data: Data =
                    { Token = testContext.Value.GetResetPasswordToken()
                      Password = "1234A!sd" }
                let! res = testFixture.HttpPostAsync' "/api/auth/reset-password/confirm" data
                do! ensureSuccessAsync res
            }

        [<Fact>]
        [<Priority(3)>]
        member __.``C Reset password this the same confirm token must fail``() =
            task {
                let data: Data =
                    { Token = testContext.Value.GetResetPasswordToken()
                      Password = "1234A!sd" }
                let! res = testFixture.HttpPostAsync' "/api/auth/reset-password/confirm" data
                ensureUnauthorized res
            }

        [<Fact>]
        [<Priority(4)>]
        member __.``D Send reset password to unexistent user email should fail``() =
            task {
                let resetPasswordData: PRR.Domain.Auth.ResetPassword.Models.Data = { Email = "unexistent@mail.com" }
                let! res = testFixture.HttpPostAsync' "/api/auth/reset-password" resetPasswordData
                ensureNotFound res
            }

        [<Fact>]
        [<Priority(5)>]
        member __.``E Update password must be success``() =
            task {
                let resetPasswordData: PRR.Domain.Auth.UpdatePassword.Models.Data =
                    { OldPassword = "1234A!sd"
                      Password = "1234A!sd8" }
                let! res = testFixture.HttpPutAsync accessToken.Value "/api/me/password" resetPasswordData
                do! ensureSuccessAsync res
            }

        [<Fact>]
        [<Priority(6)>]
        member __.``F Update password with wrong old password must fail``() =
            task {
                let resetPasswordData: PRR.Domain.Auth.UpdatePassword.Models.Data =
                    { OldPassword = "1234A!sd"
                      Password = "1234A!sd8+" }
                let! res = testFixture.HttpPutAsync accessToken.Value "/api/me/password" resetPasswordData
                ensureUnauthorized res
            }
