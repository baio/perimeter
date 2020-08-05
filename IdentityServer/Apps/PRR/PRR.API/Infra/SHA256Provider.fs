namespace PRR.API.Infra

open System.Security.Cryptography

[<AutoOpen>]
module SHA256Provider =
    
    type SHA256Provider(sha256: SHA256) =         
        interface ISHA256Provider with
            member __.GetSHA256 = getSha256Hash sha256
