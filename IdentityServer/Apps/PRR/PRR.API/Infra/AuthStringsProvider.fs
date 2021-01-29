namespace PRR.API.Infra

open System
open PRR.Domain.Models
open System.Security.Cryptography

[<AutoOpen>]
module RandomStringProvider =
    let private random = Random()

    let private getRandomString length =
        let chars =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"

        [ 0 .. length ]
        |> Seq.map (fun x -> chars.[random.Next(chars.Length)])
        |> System.String.Concat


    let private authStringsGetter: AuthStringsGetter =
        { ClientId = fun () -> getRandomString 33
          ClientSecret = fun () -> getRandomString 50
          // TODO : Read ???
          AuthorizationCode = fun () -> getRandomString 35
          HS256SigningSecret = fun () -> getRandomString 35
          RS256XMLParams =
              fun () ->
                  let rsa = RSA.Create(2048)
                  rsa.ToXmlString(true) }

    type AuthStringsProvider() =
        interface IAuthStringsProvider with
            member __.AuthStringsGetter = authStringsGetter
