namespace PRR.Domain.Auth.Social.SocialCallback.Identities.Google

open System.Security.Claims
open Common.Domain.Models
open Newtonsoft.Json
open PRR.Data.Entities
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Domain.Auth.Common
open DataAvail.Http.Exceptions

// https://developers.google.com/identity/protocols/oauth2/web-server

module public Models =

    type CodeResponse =
        { access_token: string
          id_token: string }

    type UserResponse =
        { id: int
          picture: string
          email: string
          name: string
          given_name: string
          family_name: string }

module private Helpers =

    open Models

    let getCodeResponse (httpRequestFun: HttpRequestFun) clientId secret code redirectUri =
        task {
            let request: HttpRequest =
                { Uri = "https://oauth2.googleapis.com/token"
                  Method = HttpRequestMethodPOST
                  QueryStringParams = Seq.empty
                  FormBody =
                      seq {
                          ("client_id", clientId)
                          ("client_secret", secret)
                          ("code", code)
                          ("grant_type", "authorization_code")
                          ("redirect_uri", redirectUri)
                      }
                  Headers =
                      seq {
                          ("Accept", "application/json")
                          ("Content-Type", "application/x-www-form-urlencoded")
                      } }

            let! content = httpRequestFun request

            return JsonConvert.DeserializeObject<CodeResponse> content
        }


    let getClaimValue (claims: Claim seq) key =
        claims
        |> Seq.tryFind (fun claim -> claim.Type = key)
        |> function
        | Some x -> x.Value
        | None ->
            raise
                (key
                 |> sprintf "Claim %s is not found"
                 |> unexpected)

    let mapIdTokenToIdentity =
        readToken
        >> function
        | Some token ->
            let getClaimValue = getClaimValue token.Claims
            let email = getClaimValue "email"
            let name = getClaimValue "name"
            let sub = getClaimValue "sub"
            let socialName = socialType2Name SocialType.Google
            SocialIdentity(Name = name, Email = email, SocialName = socialName, SocialId = sub)
        | None -> raise (unexpected "id token is in wrong format")

module internal Handler =

    open Helpers

    let getSocialIdentity redirectUri httpRequestFun socialClientId socialSecretKey code =
        task {
            // request social access token by clientId, secret and code from callback
            let! codeResponse = getCodeResponse httpRequestFun socialClientId socialSecretKey code redirectUri

            // get github user by received access token
            return mapIdTokenToIdentity codeResponse.id_token
        }
