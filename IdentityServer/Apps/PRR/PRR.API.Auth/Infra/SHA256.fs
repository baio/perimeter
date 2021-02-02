namespace PRR.API.Auth.Infra

open System
open System.Security.Cryptography

[<AutoOpen>]
module SHA256 =

    let getSha256Base64Hash sha256 = getSha256Hash sha256 >> Convert.ToBase64String

    type SHA256Provider(sha256: SHA256) =
        interface ISHA256Provider with
            member __.GetSHA256 = getSha256Base64Hash sha256
