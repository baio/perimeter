namespace Common.Test.Utils

// Don't autoformat it, fantomas will strip override keyword !

[<AutoOpen>]
module TestOutputConverter =

    open System.IO
    open Xunit.Abstractions

    type TestOutputConverter(output: ITestOutputHelper) =
        inherit TextWriter()
        override __.Encoding = stdout.Encoding
        override __.WriteLine message =
            output.WriteLine message
        override __.Write message =
            output.WriteLine message

    let setConsoleOutput (output: ITestOutputHelper) =
        new TestOutputConverter(output) |> System.Console.SetOut