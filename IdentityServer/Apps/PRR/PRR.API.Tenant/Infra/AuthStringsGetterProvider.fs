namespace PRR.API.Tenant.Infra

open PRR.Domain.Tenant


type IAuthStringsGetterProvider =
    abstract AuthStringsGetter: IAuthStringsGetter

[<AutoOpen>]
module AuthStringsGetterProvider =

    open System
    open System.Security.Cryptography

    let private random = Random()

    let private getRandomString length =
        let chars =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"

        [ 0 .. length ]
        |> Seq.map (fun x -> chars.[random.Next(chars.Length)])
        |> System.String.Concat


    let private authStringsGetter: IAuthStringsGetter =
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
        interface IAuthStringsGetterProvider with
            member __.AuthStringsGetter = authStringsGetter
