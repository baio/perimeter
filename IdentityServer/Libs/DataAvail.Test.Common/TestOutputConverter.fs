namespace DataAvail.Test.Common

// Don't autoformat it, fantomas will strip override keyword !

[<AutoOpen>]
module TestOutputConverter =

    open System.IO
    open Xunit.Abstractions

    type TestOutputConverter(output: ITestOutputHelper) =
        inherit TextWriter()
        override __.Encoding = stdout.Encoding
        override __.WriteLine message = output.WriteLine message

        override __.Write message =
            try
                output.WriteLine message
            with _ -> ()

    let setConsoleOutput (output: ITestOutputHelper) =
        new TestOutputConverter(output)
        |> System.Console.SetOut
