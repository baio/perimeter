namespace PRR.API.Tenant.Infra

open System.Web
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


    let private joinUri (a: string) (b: string) =
        Uri(Uri(a), b).ToString()

    let authStringsGetter (baseUri: string): IAuthStringsGetter =
        { ClientId = fun () -> getRandomString 33
          ClientSecret = fun () -> getRandomString 50
          // TODO : Read ???
          AuthorizationCode = fun () -> getRandomString 35
          HS256SigningSecret = fun () -> getRandomString 35
          RS256XMLParams =
              fun () ->
                  let rsa = RSA.Create(2048)                  
                  rsa.ToXmlString(true)
          GetIssuerUri = fun data ->
              joinUri baseUri (sprintf "issuers/%s/%s/%s" data.TenantName data.DomainName data.EnvName)

          GetAudienceUri =
              fun data ->
                  joinUri
                      baseUri
                      (sprintf
                          "audiences/%s/%s/%s/%s"
                           data.IssuerUriData.TenantName
                           data.IssuerUriData.DomainName
                           data.IssuerUriData.EnvName
                           data.ApiName)
        }


    type AuthStringsProvider(authStringsGetter) =
        interface IAuthStringsGetterProvider with
            member __.AuthStringsGetter = authStringsGetter
