namespace PRR.API.Tests.Utils

open Common.Test.Utils

[<AutoOpen>]
module TestFixture =
    
    type TestFixture() =                    
        inherit ClientFixture((fun () -> createHost' true))





