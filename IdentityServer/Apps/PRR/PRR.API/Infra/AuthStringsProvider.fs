namespace PRR.API.Infra

open System
open Common.Domain.Models

[<AutoOpen>]
module RandomStringProvider =
    let private random = Random()

    let private getRandomString length =
        let chars =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"
        [ 0 .. length ]
        |> Seq.map (fun x -> chars.[random.Next(chars.Length)])
        |> System.String.Concat


    let private authStringsProvider: AuthStringsProvider =
        { ClientId = fun () -> getRandomString 33
          ClientSecret = fun () -> getRandomString 50
          // TODO : Read ???
          AuthorizationCode = fun () -> getRandomString 35 }

    type AuthStringsProvider() =
        interface IAuthStringsProvider with
            member __.AuthStringsProvider = authStringsProvider
