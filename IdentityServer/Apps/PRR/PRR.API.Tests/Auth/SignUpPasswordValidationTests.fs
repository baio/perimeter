﻿namespace PRR.API.Tests

open Akkling
open Common.Domain.Models
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
open PRR.Domain.Auth.Utils

module SignUpPasswordValidation =
    ()




[<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
type ``sign-up-password-validation-api``(testFixture: TestFixture, output: ITestOutputHelper) =
    do setConsoleOutput output
    interface IClassFixture<TestFixture>

    [<Fact>]
    member __.``only low chars must fails``() =
        let pass = "gswfhjgjhergfjh"

        let actual =
            validatePassword pass
            |> Array.filter (fun x -> x.IsSome)

        let expected =
            [ Some(BadRequestFieldError("password", PASSWORD)) ]

        actual |> should equal expected

    [<Fact>]
    member __.``only digits must fails``() =
        let pass = "12567576215376"

        let actual =
            validatePassword pass
            |> Array.filter (fun x -> x.IsSome)

        let expected =
            [ Some(BadRequestFieldError("password", PASSWORD)) ]

        actual |> should equal expected

    [<Fact>]
    member __.``at least one char and one digit``() =
        let pass = "a12567576215376"

        let actual =
            validatePassword pass
            |> Array.filter (fun x -> x.IsSome)

        actual |> should equal []
