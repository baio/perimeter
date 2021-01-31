namespace PRR.API.Tests.Utils

open DataAvail.Test.Common

[<AutoOpen>]
module TestFixture =
    
    type TestFixture() =                    
        inherit ClientFixture((fun () -> createHost' true), (fun () -> createHost' true))





