namespace PRR.API.Tests.Utils

open DataAvail.Test.Common

[<AutoOpen>]
module TestFixture =
    
    type TestFixture() =                    
        inherit ClientFixture(createAuthHost, createTenantHost)





