namespace PRR.API.Tests
open Common.Domain.Models
open Common.Test.Utils
open DataAvail.Http.Exceptions
open FsUnit
open Xunit
open Xunit.Abstractions
open Xunit.Priority
open PRR.Domain.Auth.Utils

module SignUpPasswordValidation =
    ()

[<TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)>]
type ``sign-up-password-validation-api``(output: ITestOutputHelper) =
    do setConsoleOutput output

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
