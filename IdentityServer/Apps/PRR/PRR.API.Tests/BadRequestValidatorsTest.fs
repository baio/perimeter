namespace PRR.API.Tests.BadRequestValidatorsTests

open Xunit
open Xunit.Abstractions
open Xunit.Priority
open Common.Test.Utils
open Common.Domain.Utils.BadRequestValidators
open FsUnit


module BadRequestValidatorsTests =

    type ``bad-request-validators-api``(output: ITestOutputHelper) =
        do setConsoleOutput output

        [<Fact>]
        member __.``Validate contains all 1``() =
            let input = [ "openid"; "profile" ]
            let expect = [ "openid"; "profile" ]
            let actual = validateContainsAll'' expect input
            actual |> should equal true


        [<Fact>]
        member __.``Validate contains all 2``() =
            let input = [ "openid"; "profile"; "email" ]
            let expect = [ "openid"; "profile" ]
            let actual = validateContainsAll'' expect input
            actual |> should equal true

        [<Fact>]
        member __.``Validate contains all 3``() =
            let input = [ "openid" ]
            let expect = [ "openid"; "profile" ]
            let actual = validateContainsAll'' expect input
            actual |> should equal false
