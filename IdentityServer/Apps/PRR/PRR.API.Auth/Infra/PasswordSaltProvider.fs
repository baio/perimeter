namespace PRR.API.Infra

[<AutoOpen>]
module PasswordSaltProvider =

    open System.Text
    open System.Security.Cryptography

    let getHMACSHA256 (hmac: HMACSHA256): string -> string = 
        Encoding.UTF8.GetBytes >> hmac.ComputeHash >> System.Convert.ToBase64String

    type PasswordSaltProvider(passwordSecret: string) = 
        let key = passwordSecret |> Encoding.UTF8.GetBytes
        let hmac = new HMACSHA256(key)        
        interface IPasswordSaltProvider with
            member __.SaltPassword = (getHMACSHA256 hmac)


