namespace PRR.Domain.Auth.Common

open System
open System.IdentityModel.Tokens.Jwt

[<AutoOpen>]
module ReadToken =

    let readToken (token: string) =
        let tokenHandler = JwtSecurityTokenHandler()
        try
            let securityToken = tokenHandler.ReadJwtToken(token)
            if (securityToken = null) then None else Some securityToken
        with :? Exception as ex ->
            printfn "Read token fails %O" ex
            None
